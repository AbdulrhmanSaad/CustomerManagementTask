using Microsoft.AspNetCore.RateLimiting;

namespace AuthServer.Extentions
{
    public static class RateLimitingExtestion
    {
        public static void AddRateLimiting(this WebApplicationBuilder builder)
        {
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromSeconds(60);
                    limiterOptions.QueueLimit = 0;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

        }
    }
}
