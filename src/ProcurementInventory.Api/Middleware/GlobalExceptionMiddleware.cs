using System.Net;
using System.Text.Json;
using ProcurementInventory.Api.Models;

namespace ProcurementInventory.Api.Middleware;

/// <summary>
/// 全域例外處理 Middleware
/// 捕捉所有未處理例外，記錄日誌並回傳統一錯誤格式
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var requestId = context.TraceIdentifier;
            _logger.LogError(ex, "未處理的例外。RequestId: {RequestId}, Path: {Path}",
                requestId, context.Request.Path);

            await HandleExceptionAsync(context, ex, requestId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "未授權的存取"),
            _ => (HttpStatusCode.InternalServerError, "系統發生錯誤，請聯絡管理員")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(message, requestId);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Middleware 擴充方法
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
