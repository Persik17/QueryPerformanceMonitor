using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QueryPerformanceMonitorAPI.Data.Entities;

namespace QueryPerformanceMonitorAPI.Data.Configurations
{
    public class PostgresOrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Total)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            // PostgreSQL специфичные настройки
            builder.Property(o => o.OrderDate)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.OrderDate);
        }
    }
}
