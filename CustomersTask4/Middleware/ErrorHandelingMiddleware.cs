using CustomersTask4.Exceptions;

namespace CustomersTask4.Middleware
{
    public class ErrorHandelingMiddleware(ILogger<ErrorHandelingMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException exption)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(exption.Message);
                logger.LogWarning(exption.Message);
            }
            catch(ArgumentException ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
                logger.LogWarning(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
