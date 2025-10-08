using NetPcContacts.Domain.Exceptions;

namespace NetPcContacts.Api.Middlewares
{
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
                _logger.LogError(notFound.Message);

                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(notFound.Message);
            }
            catch (DuplicateEmailException duplicate)
            {
                _logger.LogError(duplicate.Message);

                context.Response.StatusCode = 409;
                await context.Response.WriteAsync(duplicate.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Something went wrong. Please try again later.");
            }
        }
    }
}
