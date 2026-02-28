using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Data.Configurations;

public class NotificationDisabledConfiguration : IEntityTypeConfiguration<NotificationDisabled>
{
    public void Configure(EntityTypeBuilder<NotificationDisabled> builder)
    {
        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasConversion<string>();
    }
}