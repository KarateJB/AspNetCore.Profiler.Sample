using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using Microsoft.FeatureManagement;

namespace AspNetCore.Profiler.Mvc.Utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomMiniProfiler(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMiniProfiler(async options =>
            {
                options.RouteBasePath = "/profiler";

                #region Storage
                var featureManager = services.BuildServiceProvider().GetRequiredService<IFeatureManager>();
                if (await featureManager.IsEnabledAsync("EnableMiniProfilerDb"))
                {
                    options.Storage = new SqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
                }
                else
                {
                    (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);
                }
                #endregion

                #region Styles
                options.PopupRenderPosition = RenderPosition.Left;
                options.PopupMaxTracesToShow = 10;
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
                #endregion

                #region Include/Exclude tracking
                options.TrackConnectionOpenClose = true;
                options.ExcludeType("MyClass");
                options.ExcludedTypes.Add("MyClass");
                options.ExcludeAssembly("MyAssembly");
                options.ExcludeMethod("MyMethod");
                options.IgnorePath("/Home");
                #endregion

                #region Authorization
                options.ResultsAuthorize = request => request.IsAuthorizedToMiniProfiler();
                options.ResultsListAuthorize = request => request.IsAuthorizedToMiniProfiler();
                #endregion
            })
            .AddEntityFramework();

            return services;
        }
    }
}
