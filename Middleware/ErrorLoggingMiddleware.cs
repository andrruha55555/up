using ApiUp.Context;
using ApiUp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ApiUp.Middleware
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, LogsContext logsContext)
        {
            // чтобы можно было прочитать Body несколько раз
            context.Request.EnableBuffering();

            string? body = null;
            try
            {
                if (context.Request.ContentLength is > 0)
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;

                _logger.LogError(ex, "Unhandled exception. TraceId={TraceId} Path={Path}", traceId, context.Request.Path);
                try
                {
                    logsContext.ErrorLogs.Add(new ErrorLog
                    {
                        created_at = DateTime.UtcNow,
                        method = context.Request.Method,
                        path = context.Request.Path.ToString(),
                        status_code = 500,
                        trace_id = traceId,
                        user_name = context.User?.Identity?.Name,
                        message = ex.Message,
                        stack_trace = ex.StackTrace,
                        inner_exception = ex.InnerException?.Message,
                        query_string = context.Request.QueryString.ToString(),
                        request_body = body
                    });

                    await logsContext.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to write error log to DB. TraceId={TraceId}", traceId);
                }

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal server error",
                    traceId
                });
            }
        }
    }
}
