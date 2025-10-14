namespace ThermoFisher.SampleVerticalModule.Chromatography;

public class ChromatographyMiddleware
{
    private readonly RequestDelegate _next;

    public ChromatographyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Sample-Vertical-Module-Chromatography", "active");
        await _next(context);
    }
}
