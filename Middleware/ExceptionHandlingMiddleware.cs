using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TicketApp.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug("İstek istemci tarafından iptal edildi: {Path}", context.Request.Path);
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex,
                    "Yanıt başladıktan sonra hata oluştu: {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Kayıt başka bir işlem tarafından değiştirildi. Lütfen tekrar deneyin."),
            _ => (StatusCodes.Status500InternalServerError, "Beklenmeyen bir hata oluştu.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex,
                "Yakalanmamış hata: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(ex,
                "İstek hatası ({StatusCode}): {Method} {Path}",
                statusCode, context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message }, JsonOptions));
    }
}
