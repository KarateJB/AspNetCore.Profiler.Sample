using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Mvc.Utils;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using OpenTelemetry.Trace;
using AspNetCore.Profiler.Mvc.Models;

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
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<HttpRequestLogFilter>();
            });

            #region HttpClients
            services.AddHttpClient(Consts.HttpClientDemo, client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            #endregion

            #region Entity framework
            services.AddDbContext<DemoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                providerOptions => providerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
            #endregion

            #region MiniProfiler
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";

                #region Storage

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // Enable SQL Server storage
                // options.Storage = new SqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
                #endregion

                #region Styles

                // Default: left
                options.PopupRenderPosition = RenderPosition.Left; // Left|Right|BottomLeft|BottomRight

                // Default: 15
                options.PopupMaxTracesToShow = 10;


                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();

                #endregion

                #region Include/Exclude tracking

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;

                // Ignore tracing any class named "MyClass"
                options.ExcludeType("MyClass");
                options.ExcludedTypes.Add("MyClass");

                // Ignore tracing the assembly named "MyAssembly"
                options.ExcludeAssembly("MyAssembly");
                // options.ExcludedAssemblies.Add("AspNetCore.Profiler.Core");

                // Ignore tracing the method(s) named "IgnoreMe"
                options.ExcludeMethod("MyMethod");
                // options.ExcludedMethods.Add("MyMethod");

                // Ignore tracing the request with the url path
                options.IgnorePath("/Home");

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
