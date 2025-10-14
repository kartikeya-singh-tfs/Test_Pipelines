using System.Data;
using System.Data.Common;
using Dapper;
using Npgsql;
using ThermoFisher.Opal.Shared.Registration.Abstractions;

namespace ThermoFisher.Opal.Api;

/// <summary>
/// Configures database schemas and access for modules
/// </summary>
internal class ModuleDbConfigurator
{
    private readonly IReadOnlyList<ModuleRegistrationEntry> _moduleRegistrations;
    private readonly ISecretStore _secretStore;
    private readonly Func<string, string, string, DbConnection> _openConnection;

    /// <summary>
    /// Initializes a new instance of ModuleDbConfigurator with the registered modules.
    /// This constructor is internal and should only be called by ModuleRegistryBuilder.
    /// </summary>
    /// <param name="moduleRegistrations">List of module registrations from the registration phase.</param>
    /// <param name="secretStore">Interface to the secret store</param>
    internal ModuleDbConfigurator(
        IReadOnlyList<ModuleRegistrationEntry> moduleRegistrations,
        ISecretStore secretStore
    )
    {
        _moduleRegistrations = moduleRegistrations;
        _secretStore = secretStore;

        _openConnection = (connectionStringTemplate, userName, password) =>
        {
            var dataSource = CreateDataSource(connectionStringTemplate, userName, password);
            return dataSource.OpenConnection();
        };
    }

    /// <summary>
    /// Initializes a new instance of ModuleDbConfigurator with the registered modules.
    /// This constructor is for testing only.
    /// </summary>
    /// <param name="moduleRegistrations">List of module registrations from the registration phase.</param>
    /// <param name="secretStore">Interface to the secret store</param>
    /// <param name="openConnection">Delegate to open a DB connection using connection string template, userName, and password</param>
    internal ModuleDbConfigurator(
        IReadOnlyList<ModuleRegistrationEntry> moduleRegistrations,
        ISecretStore secretStore,
        Func<string, string, string, DbConnection> openConnection
    )
    {
        _moduleRegistrations = moduleRegistrations;
        _secretStore = secretStore;

        _openConnection = openConnection;
    }

    /// <summary>
    /// Returns the modules that implement <see cref="IDbRegistrationModule"/>.
    /// </summary>
    public IEnumerable<IDbRegistrationModule> DbModules
    {
        get =>
            _moduleRegistrations
                .Select(entry => entry.Module as IDbRegistrationModule)
                .Where(dbModule => dbModule != null)!;
    }

    public void Validate()
    {
        var schemaToModule = new Dictionary<string, string>();
        var keyToModule = new Dictionary<string, string>();

        foreach (var dbModule in DbModules)
        {
            string schemaName = dbModule.SchemaName.ToLowerInvariant();
            ThrowIfUnsafeForSQL(schemaName, nameof(dbModule.SchemaName));
            if (schemaToModule.TryGetValue(schemaName, out string? duplicate))
            {
                throw new InvalidOperationException(
                    $"Duplicate schema name '{schemaName}' used by modules {dbModule.Name} and {duplicate}"
                );
            }
            schemaToModule.Add(schemaName, dbModule.Name);

            if (keyToModule.TryGetValue(dbModule.ServiceKey, out duplicate))
            {
                throw new InvalidOperationException(
                    $"Duplicate service key '{dbModule.ServiceKey}' used by modules {dbModule.Name} and {duplicate}"
                );
            }
            keyToModule.Add(dbModule.ServiceKey, dbModule.Name);
        }
    }

    /// <summary>
    /// Create and initialize DB schemas for all registered modules that implement IDbRegistrationModule.
    ///
    /// The schema will be named after the module's SchemaName.
    /// Also create a dedicated user to access that schema.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment (Development, Production, etc.).</param>
    /// <exception cref="InvalidOperationException">Thrown when called after BuildApplicationConfigurator().</exception>
    public async Task CreateOrMigrateSchemasAsync(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        CancellationToken cancellationToken
    )
    {
        var connectionStringTemplate =
            configuration.GetConnectionString("opal")
            ?? throw new InvalidOperationException("Connection string 'opal' not found.");

        string password = _secretStore.GetDbAdminPassword();
        using var adminConnection = _openConnection(connectionStringTemplate, "opal", password);

        await Parallel.ForEachAsync(
            DbModules,
            async (dbModule, cancellationToken) =>
            {
                await EnsureUserAndSchemaExistAsync(adminConnection, dbModule.SchemaName);
                using var connection = _openConnection(
                    connectionStringTemplate,
                    dbModule.SchemaName,
                    _secretStore.GetDbPassword(dbModule.SchemaName)
                );

                await dbModule.InitializeOrMigrateSchemaAsync(connection, cancellationToken);
            }
        );
    }

    /// <summary>
    /// Configure DB services for all registered modules that implement IDbRegistrationModule.
    ///
    /// Specifically, register DbDataSource and DbConnection as keyed services,
    /// using the module's ServiceKey.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment (Development, Production, etc.).</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var connectionStringTemplate =
            configuration.GetConnectionString("opal")
            ?? throw new InvalidOperationException("Connection string 'opal' not found.");

        foreach (var dbModule in DbModules)
        {
            DbDataSource dataSource = CreateDataSource(connectionStringTemplate, dbModule);
            services.AddKeyedSingleton<DbDataSource>(dbModule.ServiceKey, dataSource);
            services.AddKeyedScoped<DbConnection>(
                dbModule.ServiceKey,
                (sp, key) => dataSource.OpenConnection()
            );
        }
    }

    /// <summary>
    /// Create a datasource with user set for the given module
    /// </summary>
    /// <param name="connectionStringTemplate">Template connection string (from configuration)</param>
    /// <param name="dbModule">the module to create the datasource for</param>
    /// <returns></returns>
    private DbDataSource CreateDataSource(
        string connectionStringTemplate,
        IDbRegistrationModule dbModule
    )
    {
        return CreateDataSource(
            connectionStringTemplate,
            dbModule.SchemaName,
            _secretStore.GetDbPassword(dbModule.SchemaName)
        );
    }

    /// <summary>
    /// Create a datasource for the given user and password
    /// </summary>
    /// <param name="Template connection string (from configuration)"></param>
    /// <param name="userName">User name</param>
    /// <param name="password">Corresponding password</param>
    /// <returns></returns>
    private static DbDataSource CreateDataSource(
        string connectionStringTemplate,
        string userName,
        string password
    )
    {
        var connectionString = new NpgsqlConnectionStringBuilder(connectionStringTemplate)
        {
            Username = userName,
            Password = password,
            Database = "postgres",
        }.ConnectionString;
        DbDataSource dataSource = NpgsqlDataSource.Create(connectionString);
        return dataSource;
    }

    /// <summary>
    /// Ensure that the user and schema with the given name exist,
    /// creating them if necessary.
    /// </summary>
    /// <param name="adminConnection">DB Connection with administrative permissions</param>
    /// <param name="name">User and schema name</param>
    private async Task EnsureUserAndSchemaExistAsync(DbConnection adminConnection, string name)
    {
        await EnsureUserAsync(adminConnection, name);
        await EnsureSchemaExistsAsync(adminConnection, name);
    }

    /// <summary>
    /// Ensure that the DB user with the given name exists,
    /// creating it if necessary.
    /// </summary>
    /// <param name="adminConnection">DB Connection with administrative permissions</param>
    /// <param name="name">User name</param>
    private async Task EnsureUserAsync(DbConnection adminConnection, string name)
    {
        if (!await UserExistsAsync(adminConnection, name))
        {
            await CreateUserAsync(adminConnection, name, _secretStore.CreateDbPassword(name));
        }
    }

    /// <summary>
    /// Check whether the given DB user exists
    /// </summary>
    /// <param name="adminConnection">DB Connection with administrative permissions</param>
    /// <param name="name">User name</param>
    /// <returns></returns>
    private async static Task<bool> UserExistsAsync(DbConnection adminConnection, string name)
    {
        var sql = """
            SELECT COUNT(*) FROM pg_roles WHERE rolname=@name
            """;
        return await adminConnection.ExecuteScalarAsync<int>(sql, new { name }) != 0;
    }

    /// <summary>
    /// Create a DB user with the given name
    /// </summary>
    /// <param name="adminConnection"></param>
    /// <param name="name"></param>
    /// <param name="password"></param>
    private async static Task CreateUserAsync(
        DbConnection adminConnection,
        string name,
        string password
    )
    {
        ThrowIfUnsafeForSQL(name, nameof(name));
        ThrowIfUnsafeForSQL(password, nameof(password));

        var sql = $"""
            CREATE USER {name} WITH PASSWORD '{password}';
            """;
        await adminConnection.ExecuteAsync(sql);
    }

    /// <summary>
    /// Ensure that the schema with the given name exists,
    /// creating it if necessary.
    /// </summary>
    /// <param name="adminConnection">DB Connection with administrative permissions</param>
    /// <param name="name">Schema name, and also name of the associated user</param>
    private async static Task EnsureSchemaExistsAsync(DbConnection adminConnection, string name)
    {
        ThrowIfUnsafeForSQL(name, nameof(name));

        var sql = $"""
            CREATE SCHEMA IF NOT EXISTS AUTHORIZATION {name};
            """;
        await adminConnection.ExecuteAsync(sql);
    }

    /// <summary>
    /// Check whether a string is save for interpolation in SQL.
    /// Throw an <see>ArgumentException</see> if not.
    ///
    /// Note that this is not a complete check.
    /// It rejects many characters that are actually OK in SQL,
    /// and allows SQL keywords which could be malicious in some places.
    ///
    /// The purpose of this method is only to avoid accidental generation of invalid SQL,
    /// since some PostgreSQL admin methods do not support parameter placeholders,
    /// and therefore need string interpolation.
    /// </summary>
    /// <param name="param">The string to check</param>
    /// <param name="paramName">The name of the parameter that should be shown in the exception</param>
    /// <exception cref="ArgumentException"></exception>
    private static void ThrowIfUnsafeForSQL(string param, string paramName)
    {
        if (string.IsNullOrWhiteSpace(param))
        {
            throw new ArgumentException("Parameter must not be empty", paramName);
        }
        if (!param.All<char>(c => Char.IsAsciiLetter(c) || Char.IsAsciiDigit(c)))
        {
            throw new ArgumentException(
                "Parameter may only contain ASCII letters and digits",
                paramName
            );
        }
    }
}
