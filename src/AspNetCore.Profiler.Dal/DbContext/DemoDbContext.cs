using AspNetCore.Profiler.Dal.Models;
using AspNetCore.Profiler.Dal.Utils;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Profiler.Dal
{
    public class DemoDbContext : DbContext
    {
        private readonly DbContextOptions<DemoDbContext> options = null;
        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
            this.options = options;
            // var conn = this.Database.GetDbConnection();
            // string connectionString = conn?.ConnectionString;
        }

        #region POCOs
        public DbSet<Table> Tables { get; set; }
        public DbSet<Payment> Payments { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Exclude from migrations
            modelBuilder.Entity<Table>().ToTable(nameof(Tables), t => t.ExcludeFromMigrations());
            // Seed
            modelBuilder.Seed();
        }
    }
}
