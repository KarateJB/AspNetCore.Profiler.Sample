using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Mvc.Utils;
using Microsoft.EntityFrameworkCore;
using NLog.Web;

namespace AspNetCore.Profiler.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Create MiniProfiler's profiling table
                var configuration = services.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var dbContext = services.GetRequiredService<DemoDbContext>() as DemoDbContext;

                var tableQueryRslt = dbContext.Payments.FromSqlRaw(
                    "SELECT NEWID() as Id FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'MiniProfilers'");

                var isExist = tableQueryRslt.Count() > 0;
                if (!isExist)
                {
                    using (var sqlserverStorage = new CustomSqlServerStorage(connectionString))
                    {
                        IEnumerable<string> createSqls = sqlserverStorage.CreateSqls;
                        foreach (string sql in createSqls)
                        {
                            _ = dbContext.Database.ExecuteSqlRaw(sql);
                        }
                    }
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        // Set properties and call methods on options
                    })
                    .UseStartup<Startup>();
                })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
               .UseNLog();
    }
}
