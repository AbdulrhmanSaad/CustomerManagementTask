using AuthServer.Data;
using AuthServer.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Extentions
{
    public static class OpenIdDictServiceExtentions
    {
        public static void AddOpenIdDict(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                options.UseOpenIddict();
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();
                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("api/Account/token");
                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(20));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                    options.SetIdentityTokenLifetime(TimeSpan.FromMinutes(30));


                    options.DisableSlidingRefreshTokenExpiration();

                    options.AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

                    options.AddDevelopmentSigningCertificate();

                    options.AcceptAnonymousClients();

                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();
                    //Integrates OpenIddict with ASP.NET Core pipeline.
                    //Lets you handle the token request in your controller.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();

                    options.DisableAccessTokenEncryption();

                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy =>
                    {
                        policy.WithOrigins(configuration.GetSection("ClientUrl").Value!)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });
        }
    }
}
