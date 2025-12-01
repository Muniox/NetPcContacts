using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NetPcContacts.Domain.Exceptions;

namespace NetPcContacts.Api.ExceptionHandlers
{
    /// <summary>
    /// Globalny handler wyjątków implementujący IExceptionHandler.
    /// Loguje szczegóły wyjątków i zwraca odpowiednie kody HTTP.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            LogException(httpContext, exception);

            var (statusCode, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                DuplicateEmailException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Status = statusCode,
                    Title = title,
                    Detail = exception.Message,
                    Type = exception.GetType().Name
                },
                Exception = exception
            });
        }

        /// <summary>
        /// Loguje szczegóły wyjątku wraz z kontekstem użytkownika i żądania.
        /// </summary>
        private void LogException(HttpContext context, Exception exception)
        {
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var userIP = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var endpoint = $"{context.Request.Method} {context.Request.Path}";
            var timestamp = DateTime.UtcNow;

            _logger.LogError(
                exception,
                "[{Timestamp}] Exception occurred | User: {UserId} | IP: {UserIP} | Endpoint: {Endpoint} | Message: {Message}",
                timestamp,
                userId,
                userIP,
                endpoint,
                exception.Message
            );
        }
    }
}