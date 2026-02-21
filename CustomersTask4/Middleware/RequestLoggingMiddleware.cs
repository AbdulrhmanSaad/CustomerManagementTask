
using CustomersTask4.SerilogMasking;
using Serilog;

namespace CustomersTask4.Middleware
{
    public class RequestLoggingMiddleware : IMiddleware
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

            await next(context);

            Log.Information("Response Status Code: {StatusCode}", context.Response.StatusCode);
        }
    }
}
