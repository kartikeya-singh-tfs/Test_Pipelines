using Serilog;
using Serilog.Events;

namespace ThermoFisher.Opal.Api.Extensions;

/// <summary>
/// Extension methods for configuring W3C request logging.
/// </summary>
internal static class RequestLoggingExtensions
{
    /// <summary>
    /// Adds Serilog request logging with W3C Extended Log Format enrichment.
    /// Logs are written to a separate file in structured JSON format.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseW3CRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) =>
                ex != null
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode switch
                    {
                        >= 500 => LogEventLevel.Error,
                        >= 400 => LogEventLevel.Warning,
                        _ => LogEventLevel.Information,
                    };
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var request = httpContext.Request;
                var response = httpContext.Response;

                // W3C Extended Log Format fields
                var now = DateTime.UtcNow;
                diagnosticContext.Set("Date", DateOnly.FromDateTime(now).ToString("yyyy-MM-dd"));
                diagnosticContext.Set("Time", TimeOnly.FromDateTime(now).ToString("HH:mm:ss"));
                diagnosticContext.Set(
                    "ClientIP",
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "-"
                );
                diagnosticContext.Set(
                    "ServerIP",
                    httpContext.Connection.LocalIpAddress?.ToString() ?? "-"
                );
                diagnosticContext.Set("ServerPort", httpContext.Connection.LocalPort);
                diagnosticContext.Set("Method", request.Method);
                diagnosticContext.Set("UriStem", request.Path.Value ?? "-");
                diagnosticContext.Set("UriQuery", request.QueryString.Value ?? "-");
                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("StatusCode", response.StatusCode);
                diagnosticContext.Set("UserAgent", request.Headers.UserAgent.ToString() ?? "-");
                diagnosticContext.Set("Host", request.Host.Value ?? "-");
                diagnosticContext.Set("Referer", request.Headers.Referer.ToString() ?? "-");
                diagnosticContext.Set("ContentType", response.ContentType ?? "-");

                // Calculate content length if available
                if (response.ContentLength.HasValue)
                    diagnosticContext.Set("BytesSent", response.ContentLength.Value);
                else
                    diagnosticContext.Set("BytesSent", 0);
            };

            // Custom message template for W3C format
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        return app;
    }
}
