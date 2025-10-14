using Serilog;
using Serilog.Events;
using ThermoFisher.Opal.Api.Extensions;

namespace ThermoFisher.Opal.Api;

public class Program
{
    // Main entry point for the application.
    public static void Main(string[] args)
    {
        // Create a builder to configure and run the application.
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();

        // Create shared Serilog logger instance
        var serilogLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        // Set the global logger for log calls
        Log.Logger = serilogLogger;

        // Setup Serilog services for the main application
        builder.Host.UseSerilog();

        // Create temporary logger factory using the global logger
        var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddSerilog());
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Starting Opal API application");

        // Enables configuration overrides from environment variables prefixed with Opal_.
        builder.Configuration.AddEnvironmentVariables(prefix: "Opal_");

        // Add lifecycle service
        builder.Services.AddHostedService<ModuleLifecycleService>();

        // Add gRPC services
        // Add gRPC services with logging interceptor
        builder.Services.AddGrpc(options =>
        {
            // Enable detailed error messages for gRPC services
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<Thermofisher.Opal.Api.GrpcLoggingInterceptor>();
        });

        // Module Registration Process, which consists of three phases:
        // Phase 1: Explicit Module Registration
        var moduleBuilder = new ModuleRegistryBuilder(loggerFactory);
        moduleBuilder.RegisterAllModules();

        var dbConfigurator = moduleBuilder.BuildDbConfigurator();
        dbConfigurator.Validate();

        // Build messaging configurator
        var msgConfigurator = moduleBuilder.BuildMessagingConfigurator();

        // Phase 2: Service Configuration - Commit to service configuration
        var serviceConfigurator = moduleBuilder.BuildServiceConfigurator();

        // Register services for all modules that implement IServiceRegistrationModule
        serviceConfigurator.ConfigureServices(
            builder.Services,
            builder.Configuration,
            builder.Environment
        );

        // Register services for all modules that implement IMessagingRegistrationModule
        msgConfigurator.ConfigureMessaging(
            builder.Services,
            builder.Configuration,
            builder.Environment
        );

        if (builder.Environment.IsDevelopment())
        {
            // Configure OpenAPI services for all modules that implement IOpenApiRegistrationModule
            serviceConfigurator.ConfigureOpenApiServices(
                builder.Services,
                builder.Configuration,
                builder.Environment
            );

            // Add services to enable GRPC reflection in development mode
            builder.Services.AddGrpcReflection();
        }

        dbConfigurator.RegisterServices(
            builder.Services,
            builder.Configuration,
            builder.Environment
        );
        //dbConfigurator
        //    .CreateOrMigrateSchemasAsync(
        //        builder.Services,
        //        builder.Configuration,
        //        builder.Environment,
        //        CancellationToken.None
        //    )
        //    .Wait();

        // Phase 3: Application Configuration - Commit to application configuration
        var appConfigurator = serviceConfigurator.BuildApplicationConfigurator();
        builder.Services.AddSingleton(appConfigurator);

        // Register CORS service with DI
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        logger.LogInformation("Building application");
        var app = builder.Build();
        logger.LogInformation("Application built successfully");

        // Add routing middleware
        app.UseRouting();

        // Add Serilog request logging with W3C enrichment
        app.UseW3CRequestLogging();

        // Configure middleware for all registered modules
        appConfigurator.ConfigureMiddleware(app, app.Services, builder.Environment);

        appConfigurator.RegisterEndpoints(app, app.Services, builder.Environment);

        // Register GRPC endpoints for all registered modules
        appConfigurator.RegisterGrpcEndpoints(app, app.Services, builder.Environment);

        if (builder.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // Add Swagger UI for OpenAPI documentation
            app.UseSwaggerUI(c =>
            {
                foreach (var module in appConfigurator.GetOpenApiDocuments())
                {
                    foreach (var document in module.Value)
                    {
                        c.SwaggerEndpoint($"/openapi/{document}.json", document);
                    }
                }
            });

            // Add GRPC reflection service endpoint for development
            app.MapGrpcReflectionService();
        }

        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        logger.LogInformation("Opal API startup completed, application ready");

        // Run the application.
        app.Run();
    }
}
