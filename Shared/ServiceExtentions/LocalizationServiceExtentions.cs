using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Shared.ServiceExtentions
{
    public static class LocalizationServiceExtentions
    {
        public static void AddLocalization(this IApplicationBuilder app)
        {
            var supportedCultures = new[] { "en", "ar", "ar-eg", "ar-sa" };
            var defaultCulture = supportedCultures.FirstOrDefault() ?? "en";

            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(defaultCulture)
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());

            app.UseRequestLocalization(localizationOptions);

        }
    }
}
