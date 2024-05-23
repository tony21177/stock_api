using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log query string
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        _logger.LogInformation($"Query string: {queryString}");

        // Log request body
        context.Request.EnableBuffering(); // Allows us to read the request body multiple times
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset the request body stream position for the next middleware
        _logger.LogInformation($"Request Path:{context.Request.Path},Query string:{queryString} ,Request body: {requestBody}");

        await _next(context);
    }
}
