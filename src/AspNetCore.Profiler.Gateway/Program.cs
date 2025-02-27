using AspNetCore.Profiler.Gateway.Models;
using AspNetCore.Profiler.Gateway.Services;
using Microsoft.FeatureManagement;
using NLog.Web;
using Ocelot.Cache;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using OpenTelemetry.Trace;

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
builder.Services.Configure<AppSettings>(builder.Configuration);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable OpenTelemetry
builder.Services.AddOpenTelemetry().WithTracing(
    b => b.AddAspNetCoreInstrumentation()
        //.AddHttpClientInstrumentation()
        .AddConsoleExporter()
);

// Add Ocelot configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// builder.Services.AddOcelot(builder.Configuration.GetSection("Ocelot")).AddPolly();
builder.Services.AddOcelot(builder.Configuration).AddPolly();
// FileConfiguration ocelotConfig = builder.Configuration.GetSection("Ocelot").Get<FileConfiguration>();
// builder.Configuration.AddOcelot(ocelotConfig);
// builder.Services.AddOcelot().AddPolly();

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
app.UseAuthorization();
await app.UseOcelot();
app.MapControllers();
app.Run();