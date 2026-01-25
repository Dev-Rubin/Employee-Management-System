using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EMS.MicroService.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private const int MaxRecords = 10000;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await LogExceptionToDb(dbContext, ex, context.Response.StatusCode);
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task LogExceptionToDb(AppDbContext dbContext, Exception ex, int statusCode)
        {
            string? fileName = null;
            int? lineNumber = null;

            var stackTrace = new StackTrace(ex, true);
            var frame = stackTrace.GetFrame(0);
            if (frame != null)
            {
                fileName = frame.GetFileName();  
                lineNumber = frame.GetFileLineNumber(); 
            }
            var log = new ExceptionLog
            {
                StackTrace = ex.StackTrace,
                FileName = fileName,
                LineNumber = lineNumber,
                Message = ex.Message,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };

            dbContext.ExceptionLogs.Add(log);

            var totalCount = await dbContext.ExceptionLogs.CountAsync();
            if (totalCount >= MaxRecords)
            {
                var toDelete = await dbContext.ExceptionLogs
                    .OrderBy(l => l.Timestamp)
                    .Take(totalCount - MaxRecords + 1)
                    .ToListAsync();

                dbContext.ExceptionLogs.RemoveRange(toDelete);
            }

            await dbContext.SaveChangesAsync();
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }


}
