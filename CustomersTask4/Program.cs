using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.GraphQL.Mutaion;
using CustomersTask4.GraphQL.Query;
using CustomersTask4.GRPC.Services;
using CustomersTask4.Hubs;
using CustomersTask4.IServiceExtentions;
using CustomersTask4.Mapping;
using CustomersTask4.Middleware;
using CustomersTask4.MinimalApi;
using CustomersTask4.OData.Configration;
using CustomersTask4.OData.CustomerHandlers.GetAll;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using HotChocolate.Authorization;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ProtoBuf.Grpc.Server;
using QuestPDF.Infrastructure;
using Serilog;
using Shared.ServiceExtentions;
using Shared.Services;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCodeFirstGrpc();


builder.AddServiceDefaults();
builder.Services.AddControllers().AddOData(opt =>
        opt.Select().Filter().OrderBy().Expand().Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", ODataConfig.GetEdmModel()));

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Add gRPC
builder.Services.AddGrpc();

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
        }
    });

builder.Services.AddAuthorizationBuilder();
builder.Services.AddGraphQLServer()
    .RegisterDbContextFactory<ApplicationDbContext>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization()
    .AddQueryType<CustomerManagemantQuery>()
    .AddMutationType<CustomerManagementMutaion>();

builder.Services.AddDbContextFactory<ApplicationDbContext>(lifetime: ServiceLifetime.Scoped);

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

if (!builder.Environment.Equals("Testing"))
{
    builder.AddCaching();
}

builder.Services.AddCustomOpenApi();
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
//builder.AddQuartzConfig();
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

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandelingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();
app.MapCustomerEndpoints();
app.MapHub<MessageHub>("/messagehub");
app.MapGraphQL("/graphql");

app.MapGrpcService<UserService>();

app.Run();
