namespace CWM.Adapters.Api.Middleware;

/// <summary>
/// Minimal stand-in for real Entra ID / OAuth client-credentials auth -- explicitly a stub,
/// not production auth. The important property for this assignment: it applies uniformly to
/// every request. The Blazor UI and a third-party integrator pass through the exact same
/// gate; neither gets a privileged path into the API.
/// </summary>
public sealed class ApiKeyAuthMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string _configuredApiKey;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuredApiKey = configuration["Auth:ApiKey"]
            ?? throw new InvalidOperationException("Configuration value 'Auth:ApiKey' is required.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
            providedKey != _configuredApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Missing or invalid API key." });
            return;
        }

        await _next(context);
    }
}
