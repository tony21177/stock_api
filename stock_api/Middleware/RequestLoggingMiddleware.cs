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
        // Log request body
        context.Request.EnableBuffering(); // Allows us to read the request body multiple times
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset the request body stream position for the next middleware
        // Extract Authorization header
        string authorizationHeader = context.Request.Headers["Authorization"];
        string bearerToken = string.Empty;
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            bearerToken = authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        // Log the request details
        _logger.LogInformation($"Request Path: {context.Request.Path}, Query string: {queryString}, Request body: {requestBody}, Authorization: {bearerToken}");

        await _next(context);
    }
}
