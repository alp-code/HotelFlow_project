using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelFlow.Infrastructure.Data.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomNumber)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.Status)
               .IsRequired();

        builder.HasOne(x => x.RoomType)
               .WithMany(rt => rt.Rooms)
               .HasForeignKey(x => x.RoomTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
