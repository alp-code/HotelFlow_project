namespace HotelFlow.Application.DTOs.Requests.Rooms;

public class CreateRoomRequest
{
    public string RoomNumber { get; set; } = default!;
    public Guid RoomTypeId { get; set; }
}
