namespace HotelFlow.Application.DTOs.Responses.Rooms;

public class RoomTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public string? Description { get; set; }
}