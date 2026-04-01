using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Hubs;
using CustomersTask4.IServiceExtentions;
using CustomersTask4.Mapping;
using CustomersTask4.Middleware;
using CustomersTask4.Services;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using Serilog;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddSignalR();
QuestPDF.Settings.License = LicenseType.Community;

MapsterConfig.Register();
builder.Services.AddSingleton(Mapster.TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddScoped<IUserTokenMangerService, UserTokenMangerService>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddFluentValidationAutoValidation();

builder.Services.AddAuthentication(op =>
    {
        op.DefaultAuthenticateScheme = "token";
        op.DefaultChallengeScheme = "token";
        op.DefaultScheme = "token";
    })
    .AddJwtBearer("token", op =>
    {
        var secretKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["ConnectionStrings:key"]!));

        op.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secretKey,
        };
    });
builder.Services.AddOpenApi("v2");
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

        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        document.SecurityRequirements.Add(securityRequirement);

        return Task.CompletedTask;
    });
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //var connectionString = builder.Configuration.GetConnectionString("CustomersManagmentDb");
    var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 20,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
        sqlOptions.CommandTimeout(120);
        sqlOptions.MinBatchSize(1);
    });
});
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
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IMigrateDatabases, MigrateToMongo>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddHttpContextAccessor();

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
var supportedCultures = new[] {"ar", "en", "ar-eg","ar-sa" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandelingMiddleware>();
app.MapControllers();
app.MapHub<MessageHub>("/messagehub");

app.Run();
