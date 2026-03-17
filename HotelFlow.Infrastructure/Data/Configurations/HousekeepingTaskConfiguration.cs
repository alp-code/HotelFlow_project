using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelFlow.Infrastructure.Data.Configurations;

public class HousekeepingTaskConfiguration : IEntityTypeConfiguration<HousekeepingTask>
{
    public void Configure(EntityTypeBuilder<HousekeepingTask> builder)
    {
        builder.HasKey(x => x.Id);

        // Osnovna svojstva
        builder.Property(x => x.Type)
               .IsRequired()
               .HasConversion(
                   v => v.ToString(),
                   v => (HousekeepingTaskType)Enum.Parse(typeof(HousekeepingTaskType), v));

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.Description)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        // REMOVED: builder.Property(x => x.Date) - This property doesn't exist

        builder.Property(x => x.Deadline)
               .IsRequired();

        builder.Property(x => x.CompletedAt)
               .IsRequired(false);

        builder.Property(x => x.Notes)
               .HasMaxLength(1000);

        // Veze
        builder.HasOne(x => x.Room)
               .WithMany(r => r.HousekeepingTasks)
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssignedToUser)
               .WithMany(u => u.HousekeepingTasks)
               .HasForeignKey(x => x.AssignedToUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}