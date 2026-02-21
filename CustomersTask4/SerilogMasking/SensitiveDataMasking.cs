using Serilog.Core;
using Serilog.Events;

namespace CustomersTask4.SerilogMasking
{
    public class SensitiveDataMasking
    {
        private static readonly string[] SensitiveKeys =
        {
           "Authorization",
           "Password",
           "Token",
           "ApiKey",
           "Cookie"
        };

        public static string MaskValue(string key, string value)
        {
            if (SensitiveKeys.Any(k => key.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "***MASKED***";

            return value;
        }
    }
}
