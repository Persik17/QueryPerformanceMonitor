using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QueryPerformanceMonitorAPI.Data.Entities;

namespace QueryPerformanceMonitorAPI.Data.Configurations
{
    public class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
    {
        public void Configure(EntityTypeBuilder<UserAudit> builder)
        {
            builder.HasKey(ua => ua.Id);

            builder.Property(ua => ua.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ua => ua.Details)
                .HasMaxLength(1000);

            builder.HasIndex(ua => ua.UserId);
            builder.HasIndex(ua => ua.Timestamp);
        }
    }
}
