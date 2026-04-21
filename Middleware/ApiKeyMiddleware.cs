namespace Indigo_task.Middleware;

public sealed class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string _validApiKey;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _validApiKey = config["ApiKey"]
            ?? throw new InvalidOperationException("ApiKey is not configured in appsettings.json.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey);
        _logger.LogInformation("Incoming key: '{ProvidedKey}', Expected: '{ExpectedKey}'", providedKey, _validApiKey);

        if (string.IsNullOrEmpty(providedKey) || providedKey != _validApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or missing API key.");
            return;
        }

        await _next(context);
    }
}
