using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Mvc.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace AspNetCore.Profiler.Mvc
{
    public class Startup
    {
        //private readonly AppSettings appSettings = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //this.Configuration.Bind(this.appSettings);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            #region Entity framework
            services.AddDbContext<DemoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                providerOptions => providerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
            #endregion

            #region MiniProfiler
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";

                #region Styles

                // Default: left
                options.PopupRenderPosition = RenderPosition.BottomLeft; // Left|Right|BottomLeft|BottomRight

                // Default: 15
                options.PopupMaxTracesToShow = 10;

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                #endregion

                #region Include/Exclude tracking

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = false;

                // Ignore tracing any class named "MyClass"
                options.ExcludeType("MyClass");
                // options.ExcludedTypes.Add("MyClass");

                // Ignore tracing the assembly named "AspNetCore.Profiler.Core"
                options.ExcludeAssembly("AspNetCore.Profiler.Core");
                // options.ExcludedAssemblies.Add("AspNetCore.Profiler.Core");

                // Ignore tracing the method(s) named "IgnoreMe"
                options.ExcludeMethod("IgnoreMe");
                // options.ExcludedMethods.Add("IgnoreMe");

                #endregion

                #region Authorization

                // (Optional)To control authorization, you can use the Func<HttpRequest, bool> options:
                // (default is everyone can access profilers)
                options.ResultsAuthorize = request => request.IsAuthorizedToMiniProfiler();
                options.ResultsListAuthorize = request => request.IsAuthorizedToMiniProfiler();

                #endregion
            })
            .AddEntityFramework(); // Enable Entity Framework tracking
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

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
