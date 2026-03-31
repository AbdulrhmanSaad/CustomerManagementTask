using Asp.Versioning;

namespace CustomersTask4.IServiceExtentions
{
    public static class ApiVersioningExtestion
    {
        public static void ApiVersioning(this WebApplicationBuilder builder) 
        {
            builder.Services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
            }).AddMvc()
            .AddApiExplorer(opt =>
            {
                 opt.GroupNameFormat = "'v'VVV";
                 opt.SubstituteApiVersionInUrl = true;
            });
        }
    }
}
