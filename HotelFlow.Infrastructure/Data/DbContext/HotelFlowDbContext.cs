using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelFlow.Infrastructure.Data.DbContext;

public class HotelFlowDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public HotelFlowDbContext(DbContextOptions<HotelFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        modelBuilder.Entity<Reservation>()
            .HasQueryFilter(r => !r.Guest.IsDeleted);

        modelBuilder.Entity<RefreshToken>()
            .HasQueryFilter(rt => !rt.User.IsDeleted);

        modelBuilder.Entity<HousekeepingTask>()
            .HasQueryFilter(ht =>
                ht.AssignedToUserId == null ||
                !ht.AssignedToUser!.IsDeleted);

        modelBuilder.Entity<UserProfile>()
            .HasQueryFilter(up => !up.User.IsDeleted);
    }
}