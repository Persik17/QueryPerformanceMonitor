using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QueryPerformanceMonitorAPI.Data.Entities;

namespace QueryPerformanceMonitorAPI.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
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

            builder.Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()"); // Для SQL Server

            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.OrderDate);
        }
    }
}
