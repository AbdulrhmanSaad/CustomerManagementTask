using Microsoft.AspNetCore.Localization;

namespace CustomersTask4.IServiceExtentions
{
    public static class LocalizationExtestion
    {
        public static void AddLocalization(this IApplicationBuilder app)
        {
            var supportedCultures = new[] { "ar", "en", "ar-eg", "ar-sa" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());

            app.UseRequestLocalization(localizationOptions);

        }
    }
}
