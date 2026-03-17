using HotelFlow.Application.DTOs.Requests.Rooms;
using HotelFlow.Application.DTOs.Responses.Rooms;

namespace HotelFlow.Application.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomResponseDto>> GetAllAsync();
    Task<Guid> CreateAsync(CreateRoomRequest request);
    Task UpdateAsync(Guid roomId, UpdateRoomRequest request);
    Task DeleteAsync(Guid roomId);

    Task<IEnumerable<RoomTypeResponse>> GetAllRoomTypesAsync();
    Task<Guid> CreateRoomTypeAsync(CreateRoomTypeRequest request);
    Task UpdateRoomTypeAsync(Guid roomTypeId, UpdateRoomTypeRequest request);
    Task DeleteRoomTypeAsync(Guid roomTypeId);
}

