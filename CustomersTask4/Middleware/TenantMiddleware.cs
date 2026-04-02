
using CustomersTask4.Services;
using CustomersTask4.Setting;
using Microsoft.Extensions.Options;

namespace CustomersTask4.Middleware
{
    public class TenantMiddleware : IMiddleware
    {
        private readonly ITenantService _tenantService;
        public TenantMiddleware(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.StartsWithSegments("/openapi/v1.json", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
            var tokenTenant = context.User.FindFirst("tenant")?.Value;

            if (context.Request.Headers.TryGetValue("tenant", out var tenantId))
                _tenantService.SetCurrentTenant(tenantId!);
            else  
                throw new Exception("No Tenant provided in the Request");

            //if user is not authenticated, we will let the authorization middleware handle it, and we will not block the request here.
            if (tokenTenant == null) {
                await next(context);
                return;
            }
            if (string.IsNullOrEmpty(tenantId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Bad Request: Tenant header is missing");
                return;
            }
            //check if user token tenant id equls to the tenant id provided in the request header.
            if (tokenTenant != tenantId && tokenTenant!=null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden:Invalid User token or invalid tenant id.");
                return;
            }

            await next(context);
        }
    }
}
