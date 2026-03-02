using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.IServiceExtentions;
using CustomersTask4.Mapping;
using CustomersTask4.Middleware;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
MapsterConfig.Register();
builder.Services.AddSingleton(Mapster.TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddScoped<IUserTokenMangerService, UserTokenMangerService>();

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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
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

       

        return Task.CompletedTask;
    });
});

builder.Services.AddMediator(cfg =>
{
    cfg.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddScoped<IAppMeditor, AppMediator>();
builder.Services.AddScoped<RequestLoggingMiddleware>();
builder.Services.AddScoped<ErrorHandelingMiddleware>();

builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ICustomerHistoryRepository, CustomerHistoryRepository>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentityApiEndpoints<User>()
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
          .EnableSensitiveDataLogging());

string provider = builder.Configuration["DatabaseProvidor"] ?? "Sql";

switch (provider)
{
    case "Mongo":
        builder.AddMongoSetings();
        
        break;

    case "Sql":
    default:
        builder.AddSqlSetings();
    break;
}

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
}
await app.SeedMongoRolesAsync();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandelingMiddleware>();

app.MapControllers();

app.Run();
