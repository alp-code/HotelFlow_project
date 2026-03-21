using HotelFlow.Application.DTOs.Requests.Rooms;
using HotelFlow.Application.DTOs.Responses.Rooms;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using HotelFlow.Domain.Exceptions;

namespace HotelFlow.Application.Services;

public class RoomService : IRoomService
{
    private readonly HotelFlowDbContext _context;

    public RoomService(HotelFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoomResponseDto>> GetAllAsync()
    {
        return await _context.Rooms
            .Include(r => r.RoomType)
            .Select(r => new RoomResponseDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Status = r.Status.ToString(),
                RoomType = r.RoomType.Name,
                PricePerNight = r.RoomType.PricePerNight
            })
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(CreateRoomRequest request)
    {
        var roomTypeExists = await _context.RoomTypes
            .AnyAsync(rt => rt.Id == request.RoomTypeId);

        if (!roomTypeExists)
            throw new NotFoundException("Room type not found");

        var roomExists = await _context.Rooms
            .AnyAsync(r => r.RoomNumber == request.RoomNumber);

        if (roomExists)
            throw new BadRequestException($"Room number {request.RoomNumber} already exists");

        var room = new Room(
            request.RoomNumber,
            request.RoomTypeId
        );

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return room.Id;
    }

    public async Task UpdateAsync(Guid roomId, UpdateRoomRequest request)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            throw new NotFoundException("Room not found");


        if (!Enum.TryParse<RoomStatus>(request.Status, true, out var status))
            throw new BadRequestException("Invalid status");

        room.Update(request.RoomNumber, status);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
            throw new NotFoundException("Room not found");

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RoomTypeResponse>> GetAllRoomTypesAsync()
    {
        return await _context.RoomTypes
            .Select(rt => new RoomTypeResponse
            {
                Id = rt.Id,
                Name = rt.Name,
                PricePerNight = rt.PricePerNight,
                MaxGuests = rt.MaxGuests,
                Description = rt.Description
            })
            .ToListAsync();
    }

    public async Task<Guid> CreateRoomTypeAsync(CreateRoomTypeRequest request)
    {
        var roomType = new RoomType(
            request.Name,
            request.PricePerNight,
            request.MaxGuests,
            request.Description
        );

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        return roomType.Id;
    }

    public async Task UpdateRoomTypeAsync(Guid roomTypeId, UpdateRoomTypeRequest request)
    {
        var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
        if (roomType == null)
            throw new NotFoundException("Room type not found");

       
        roomType.UpdateDetails(
            request.Name,
            request.PricePerNight,
            request.MaxGuests,
            request.Description
        );

        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoomTypeAsync(Guid roomTypeId)
    {
        var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
        if (roomType == null)
            throw new NotFoundException("Room type not found");

        // Provera da li postoje sobe ovog tipa
        var hasRooms = await _context.Rooms.AnyAsync(r => r.RoomTypeId == roomTypeId);
        if (hasRooms)
            throw new BadRequestException("Cannot delete room type because there are rooms assigned to it.");

        _context.RoomTypes.Remove(roomType);
        await _context.SaveChangesAsync();
    }
}
