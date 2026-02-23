
using CustomersTask4.SerilogMasking;
using CustomersTask4.Users;
using Serilog;
using System.Security.Claims;

namespace CustomersTask4.Middleware
{
    public class RequestLoggingMiddleware(IUserContext user) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var request = context.Request;

            // Log request info
            Log.Information("HTTP Request {Method} {Body} at {Time}",
                request.Method,
                request.Body,

                DateTime.UtcNow);

            var headers = request.Headers.ToDictionary(
               h => h.Key,
              h => SensitiveDataMasking.MaskValue(h.Key, h.Value.ToString())
              );
            Log.Information("Request Headers: {@Headers}", headers);
            var current=user.GetCurrentUser();
            if(current != null)
                Log.Information("Request User: {@current}",current);

            await next(context);

            Log.Information("Response Status Code: {StatusCode}", context.Response.StatusCode);
        }
    }
}
