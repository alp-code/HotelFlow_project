using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace HotelFlow.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(HotelFlowDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!context.Users.Any())
        {
            var staff = new User(
                email: "staff@hotelflow.com",
                passwordHash: "HASHED_PASSWORD",
                role: UserRole.Staff
            );

            context.Users.Add(staff);
            await context.SaveChangesAsync();
        }

        if (!context.RoomTypes.Any())
        {
            var single = new RoomType("Single", 50m, 1,"Single bed room");
            var doubleRoom = new RoomType("Double", 80m, 2, "Double bed room");

            context.RoomTypes.AddRange(single, doubleRoom);
            await context.SaveChangesAsync();

            context.Rooms.AddRange(
                new Room("101", single.Id),
                new Room("102", single.Id),
                new Room("201", doubleRoom.Id)
            );

            await context.SaveChangesAsync();
        }
    }
}
