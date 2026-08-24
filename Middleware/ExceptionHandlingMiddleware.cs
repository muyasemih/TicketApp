using System.Net;
using System.Text.Json;

namespace TicketApp.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception )
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Beklenmeyen bir hata oluştu."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}