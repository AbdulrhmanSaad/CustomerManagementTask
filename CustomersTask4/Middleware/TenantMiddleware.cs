
namespace CustomersTask4.Middleware
{
    public class TenantMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var tokenTenant = context.User.FindFirst("tenant")?.Value;
            var requestTenant = context.Request.Headers["tenant"].ToString();

            //if user is not authenticated, we will let the authorization middleware handle it, and we will not block the request here.
            if (tokenTenant == null) {
                await next(context);
            }
            if (requestTenant == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Bad Request: Tenant header is missing");
                return;
            }
            
            if (tokenTenant != requestTenant&&tokenTenant!=null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden:Invalid User token or invalid tenant id.");
                return;
            }

            await next(context);
        }
    }
}
