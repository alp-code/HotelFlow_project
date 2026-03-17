namespace HotelFlow.Application.DTOs.Responses.Rooms;

public class RoomResponseDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string RoomType { get; set; } = default!;
    public decimal PricePerNight { get; set; }
}