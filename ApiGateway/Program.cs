using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
          var authUrl = builder.Configuration.GetConnectionString("AuthURL");
          if (!string.IsNullOrWhiteSpace(authUrl))
          {
              options.Authority = authUrl;
              options.Audience = "resource-server";
              options.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateAudience = true,
                  ValidateIssuer = true,
                  ValidateIssuerSigningKey = true
              };
              options.Events = new JwtBearerEvents
              {
           
                  OnAuthenticationFailed = context =>
                  {
                      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                      return context.Response.WriteAsJsonAsync(new { error = context.Exception.Message });
                  },
                  OnTokenValidated = context =>
                  {
                      return Task.CompletedTask;
                  }
              };
          }
      });


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
