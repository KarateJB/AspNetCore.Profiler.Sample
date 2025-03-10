using AspNetCore.Profiler.Gateway.Models;
using AspNetCore.Profiler.Gateway.Services;
using Microsoft.Extensions.Hosting;
using AspNetCore.Profiler.Gateway.Externsions;
using Microsoft.FeatureManagement;
using NLog.Web;
using Ocelot.Cache;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

// Add feature management
builder.Services.AddFeatureManagement();

// Add services to the container
var featureManager = builder.Services.BuildServiceProvider().GetRequiredService<IFeatureManager>();
if (await featureManager.IsEnabledAsync(nameof(FeatureFlags.OcelotCaching)))
{
    builder.Services.AddSingleton<IOcelotCache<CachedResponse>, RedisCacheStore>();
}

// Add Logging
builder.Host.UseNLog();
builder.Services.AddLogging(b =>
{
    b.AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddConsole()
        .AddDebug();
});

// Add configuration
// builder.Host.AddBasicConfiguration(args); // Basic configuration
builder.Host.AddCustomConfiguration(args); // Loading and rendering configuration with environment variables
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable OpenTelemetry
builder.Services.AddOpenTelemetry().WithTracing(
    b => b.AddAspNetCoreInstrumentation()
        //.AddHttpClientInstrumentation()
        .AddConsoleExporter()
);

// Add JWT Authentication
const string AuthenticationProviderKey = "ApiGateway";
var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSetting>();
var key = Encoding.ASCII.GetBytes(jwtSetting.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(AuthenticationProviderKey, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSetting.Issuer,
        ValidAudience = jwtSetting.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add Ocelot configuration file
if (await featureManager.IsEnabledAsync(nameof(FeatureFlags.TemplateConfig)))
{
    FileConfiguration ocelotConfig = builder.Configuration.GetSection("Ocelot").Get<FileConfiguration>();
    builder.Configuration.AddOcelot(ocelotConfig);
    builder.Services.AddOcelot().AddPolly();
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
    builder.Services.AddOcelot(builder.Configuration).AddPolly();
}

builder.Services.AddControllers(options =>
{
    // options.Filters.Add<T>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// await app.UseOcelot();
Func<HttpContext, bool> enableOcelotWhen = (ctx) => 
    ctx.Request.Path.StartsWithSegments("/payment") || ctx.Request.Path.StartsWithSegments("/demo");
app.MapWhen(enableOcelotWhen, (app) =>
{
    app.UseOcelot().Wait();
});

app.MapControllers();
app.Run();
