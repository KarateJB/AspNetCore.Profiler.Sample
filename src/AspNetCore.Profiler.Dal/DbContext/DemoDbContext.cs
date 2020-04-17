using System;
using System.Collections.Generic;
using System.Text;
using AspNetCore.Profiler.Dal.Models;
using AspNetCore.Profiler.Dal.Utils;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Profiler.Dal
{
    public class DemoDbContext : DbContext
    {
        private readonly DbContextOptions<DemoDbContext> options = null;
        public DemoDbContext(DbContextOptions<DemoDbContext> options): base(options)
        {
            this.options = options;
        }

        #region POCOs
        public DbSet<Payment> Payments { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed
            modelBuilder.Seed();
        }
    }
}
