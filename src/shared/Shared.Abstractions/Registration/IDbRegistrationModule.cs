using System.Data.Common;

namespace ThermoFisher.Opal.Shared.Registration.Abstractions;

/// <summary>
/// Interface for modules that need to register database connections
/// </summary>
public interface IDbRegistrationModule : IModule
{
    /// <summary>
    /// Initialize the database schema, and/or migrate it to the latest version
    ///
    /// This may include seeding it with data
    ///
    /// <param name="dbConnection">
    /// The database connection to use.
    ///
    /// The connection is open and has sufficient privileges to create tables.
    /// </summary>
    Task InitializeOrMigrateSchemaAsync(
        DbConnection dbConnection,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Unique key for services registered specifically for this module.
    ///
    /// Use with <c>[FromKeyedServices]</> attribute.
    /// </summary>
    string ServiceKey { get; }

    /// <summary>
    /// Name of the DB Schema associated with this module.
    /// This is also used as DB user name.
    /// Must only contain lowercase ASCII letters and underscore.
    /// </summary>
    string SchemaName
    {
        get => Name.ToLowerInvariant();
    }
}
