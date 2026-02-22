using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Mapping;
using CustomersTask4.Middleware;
using CustomersTask4.Repository;
using CustomersTask4.SerilogMasking;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Security.Cryptography.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddFluentValidationAutoValidation();

builder.Services.AddAuthentication(op => op.DefaultAuthenticateScheme = "token")
    .AddJwtBearer("token", op =>
    {
        var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("this is my secret key abdo saad key"));

        op.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secretKey,
        };

    });



builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"

    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                 Reference = new OpenApiReference
                  {
                      Type = ReferenceType.SecurityScheme,
                      Id="bearerAuth"
                  }

            },new List<string>()
        }
    });
});

builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>));
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddScoped<RequestLoggingMiddleware>();
builder.Services.AddScoped<ErrorHandelingMiddleware>();

builder.Services.AddScoped<IUserContext,UserContext>();
builder.Services.AddScoped<ICustomerHistoryRepository,CustomerHistoryRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentityApiEndpoints<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(
    option=>option
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()
    );
builder.Host.UseSerilog((context, config) =>
{
    config.
    ReadFrom.Configuration(context.Configuration);
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");

        //options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });

}
app.UseSerilogRequestLogging();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandelingMiddleware>();

app.UseHttpsRedirection();
app.MapGroup("/api/Identity").MapIdentityApi<User>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
