using AuthServer.Services;
using Shared.Services;

namespace AuthServer.Middleware
{
        public class TenantMiddleware : IMiddleware
        {
            private readonly ITenantService _tenantService;
            private readonly ILocalizationService _localization;
            public TenantMiddleware(ITenantService tenantService,ILocalizationService localization)
            {
                _tenantService = tenantService;
                _localization = localization;
            }
            public async Task InvokeAsync(HttpContext context, RequestDelegate next)
            {
                if (context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);
                    return;
                }
                var tokenTenant = context.User.FindFirst("tenant")?.Value;

                if (context.Request.Headers.TryGetValue("tenant", out var tenantId))
                    _tenantService.SetCurrentTenant(tenantId!);
                else
                    throw new Exception(_localization.Localize("No Tenant provided in the Request"));

            if (tokenTenant == null)
            {
                await next(context);
                return;
            }
            if (string.IsNullOrEmpty(tenantId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(_localization.Localize("Bad Request: Tenant header is missing"));
                    return;
                }
                //check if user token tenant id equls to the tenant id provided in the request header.
                if (tokenTenant != tenantId && tokenTenant != null)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync(_localization.Localize("Forbidden:Invalid User token or invalid tenant id."));
                    return;
                }

                await next(context);
            }
        }
}
