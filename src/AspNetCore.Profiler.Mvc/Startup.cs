using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Mvc.Utils;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using OpenTelemetry.Trace;
using AspNetCore.Profiler.Mvc.Models;
using Microsoft.FeatureManagement;

namespace AspNetCore.Profiler.Mvc
{
    public class Startup
    {
        //private readonly AppSettings appSettings = null;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            //this.Configuration.Bind(this.appSettings);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Feature Management
            services.AddFeatureManagement();
            #endregion

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<HttpRequestLogFilter>();
            });

            #region HttpClients
            services.AddHttpClient(Consts.HttpClientDemo, client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            #endregion

            #region Entity framework
            services.AddDbContext<DemoDbContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
               providerOptions => providerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
            #endregion

            #region MiniProfiler
            services.AddCustomMiniProfiler(Configuration);
            #endregion

            #region OpenTelemetry
            services.AddOpenTelemetry().WithTracing(
            builder => builder
                .AddAspNetCoreInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter());
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Miniprofiler
            app.UseMiniProfiler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
