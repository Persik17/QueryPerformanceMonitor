using Microsoft.EntityFrameworkCore;
using QueryPerformanceMonitorAPI.Data.Configurations;
using QueryPerformanceMonitorAPI.Data.Entities;

namespace QueryPerformanceMonitorAPI.Data
{
    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserAudit> UserAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Применяем конфигурации
            modelBuilder.ApplyConfiguration(new PostgresUserConfiguration());
            modelBuilder.ApplyConfiguration(new PostgresOrderConfiguration());
            modelBuilder.ApplyConfiguration(new PostgresUserAuditConfiguration());
        }
    }
}
