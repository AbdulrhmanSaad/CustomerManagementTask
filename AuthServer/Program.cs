using AuthServer.Extentions;
using AuthServer.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Wolverine;
using Shared.ServiceExtentions;
using Shared.Services;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddLocalization(opt => { opt.ResourcesPath = "Resource"; });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddMultiTenancy(builder.Configuration);
builder.Services.AddOpenIdDict(builder.Configuration);
builder.Host.UseWolverine();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddFluentValidationAutoValidation();
builder.Services.AddScoped<TenantMiddleware>();
var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.AddLocalization();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

app.Run();
