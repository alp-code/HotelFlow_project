using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelFlow.Infrastructure.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(x => x.Id);

        // Osnovna svojstva
        builder.Property(x => x.CheckInDate)  // Changed from DateFrom
               .IsRequired()
               .HasColumnName("DateFrom"); // Eksplicitno navođenje naziva kolone

        builder.Property(x => x.CheckOutDate) // Changed from DateTo
               .IsRequired()
               .HasColumnName("DateTo");

        builder.Property(x => x.NumberOfGuests)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(x => x.SpecialRequests)
               .HasMaxLength(1000);

        builder.Property(x => x.TotalPrice)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        // REMOVED: builder.Property(x => x.PricePerNightAtBooking) - This property doesn't exist

        builder.Property(x => x.IsPaid)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.CheckedInAt)
               .IsRequired(false);

        builder.Property(x => x.CheckedOutAt)
               .IsRequired(false);

        builder.Property(x => x.HousekeepingTaskId)
               .IsRequired(false);

        // Veza sa Guest (User)
        builder.HasOne(x => x.Guest)
               .WithMany(u => u.Reservations)
               .HasForeignKey(x => x.GuestId)
               .OnDelete(DeleteBehavior.Restrict);

        // Veza sa Room
        builder.HasOne(x => x.Room)
               .WithMany(r => r.Reservations)
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Restrict);

        // Veza sa HousekeepingTask 
        builder.HasOne(x => x.HousekeepingTask)
               .WithOne()
               .HasForeignKey<Reservation>(x => x.HousekeepingTaskId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}