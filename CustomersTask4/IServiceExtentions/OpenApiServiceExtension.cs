using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CustomersTask4.IServiceExtentions
{
    public static class OpenApiServiceExtension
    {
        public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi("v1", options =>
            {
                Configure(options, "API V1");
            });

            services.AddOpenApi("v2", options =>
            {
                Configure(options, "API V2");
            });

            return services;
        }

        private static void Configure(OpenApiOptions options, string title)
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = title,
                    Version = context.DocumentName
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["bearerAuth"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    }
                };

                var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "bearerAuth"
                        }
                    },
                    new List<string>()
                }
            };

                document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
                document.SecurityRequirements.Add(securityRequirement);

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, ct) =>
            {
                operation.Parameters ??= new List<OpenApiParameter>();

                if (!operation.Parameters.Any(p => p.Name == "tenant"))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "tenant",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Default = new OpenApiString("SharedTenant")
                        }
                    });
                }

                return Task.CompletedTask;
            });
        }
    }
}
