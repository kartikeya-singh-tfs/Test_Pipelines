namespace ThermoFisher.SampleOnionModule.Registration;

// Optional: Can be used to run the SampleOnionModule as a standalone application, outside OPAL
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddSampleOnionModule(builder.Configuration);

        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Map endpoints
        app.MapSampleOnionModuleEndpoints();

        // Configure OpenAPI
        app.MapOpenApi();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/openapi/v1.json", "Sample Onion Module API V1");
        });

        // Run the application
        app.Run();
    }
}
