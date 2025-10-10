using NetPcContacts.Domain.Exceptions;

namespace NetPcContacts.Api.Middlewares
{
    /// <summary>
    /// Middleware do globalnej obsługi błędów w aplikacji.
    /// Loguje szczegóły wyjątków i zwraca odpowiednie kody HTTP.
    /// </summary>
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException notFound)
            {
                LogException(context, notFound, "Resource not found");

                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(notFound.Message);
            }
            catch (DuplicateEmailException duplicate)
            {
                LogException(context, duplicate, "Duplicate email attempt");

                context.Response.StatusCode = 409;
                await context.Response.WriteAsync(duplicate.Message);
            }
            catch (Exception ex)
            {
                LogException(context, ex, "Unhandled exception");

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Something went wrong. Please try again later.");
            }
        }

        /// <summary>
        /// Loguje szczegóły wyjątku wraz z kontekstem użytkownika i żądania.
        /// </summary>
        private void LogException(HttpContext context, Exception exception, string eventName)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var userIP = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var endpoint = $"{context.Request.Method} {context.Request.Path}";
            var timestamp = DateTime.UtcNow;

            _logger.LogError(
                exception,
                "[{Timestamp}] {EventName} | User: {UserId} | IP: {UserIP} | Endpoint: {Endpoint} | Message: {Message}",
                timestamp,
                eventName,
                userId,
                userIP,
                endpoint,
                exception.Message
            );
        }
    }
}
