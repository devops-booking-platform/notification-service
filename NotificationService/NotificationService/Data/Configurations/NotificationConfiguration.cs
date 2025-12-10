using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(p => p.Message)
            .IsRequired()
            .HasMaxLength(Notification.MessageMaxlength);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasConversion<string>();
    }
}