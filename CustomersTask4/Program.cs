using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Hubs;
using CustomersTask4.IServiceExtentions;
using CustomersTask4.Mapping;
using CustomersTask4.Middleware;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using Serilog;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);



builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

QuestPDF.Settings.License = LicenseType.Community;

MapsterConfig.Register();
builder.Services.AddSingleton(Mapster.TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddScoped<IUserTokenMangerService, UserTokenMangerService>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddFluentValidationAutoValidation();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration.GetConnectionString("AuthURL");
        options.Audience = "resource-server";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true
        };
        });

builder.Services.AddHybridCache(opt =>
{
    opt.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddCustomOpenApi();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});
builder.Services.AddScoped<IRedisCachingService, RedisCachingService>();

builder.Services.AddDistributedSqlServerCache(option =>
{
    option.ConnectionString = builder.Configuration.GetConnectionString("sqlCach");
    option.SchemaName="dbo";
    option.TableName = "CachEntries";
});

builder.AddRedisClient("redis");
//builder.AddMongoDBClient("mongo-db");

builder.Services.Configure<MongoDbSetting>(
    builder.Configuration.GetSection("MongoDbSetting"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var s = builder.Configuration
        .GetSection("MongoDbSetting")
        .Get<MongoDbSetting>();
    return new MongoClient(s?.ConnectionString);
});




builder.Services.AddLocalization(opt => { opt.ResourcesPath = "Resource"; });
builder.Services.AddScoped<IAppMeditor, AppMediator>();
builder.Services.AddScoped<RequestLoggingMiddleware>();
builder.Services.AddScoped<ErrorHandelingMiddleware>();
builder.Services.AddScoped<TenantMiddleware>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IMigrateDatabases, MigrateToMongo>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager();




builder.Services.AddMultiTenancy(builder.Configuration);
builder.AddQuartzConfig();
builder.AddWolverineConfig();
builder.AddRateLimiting();
builder.ApiVersioning();



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

app.MapDefaultEndpoints();


app.AddLocalization();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Seed roles based on provider
    if (provider == "Mongo")
    {
        await app.SeedMongoRolesAsync();
    }
    else
    {
        await app.SeedSqlRolesAsync();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database");
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v2.json", "My API v2");

    options.SwaggerEndpoint("/openapi/v1.json", "My API v1");

});

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandelingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();
app.MapHub<MessageHub>("/messagehub");

app.Run();
