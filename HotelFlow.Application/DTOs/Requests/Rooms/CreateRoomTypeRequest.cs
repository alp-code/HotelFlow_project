namespace HotelFlow.Application.DTOs.Requests.Rooms;

public class CreateRoomTypeRequest
{
    public string Name { get; set; } = default!;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public string? Description { get; set; }
}