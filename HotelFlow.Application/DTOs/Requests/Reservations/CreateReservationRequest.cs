
namespace HotelFlow.Application.DTOs.Requests.Reservations;

public class CreateReservationRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = default!;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string? SpecialRequests { get; set; }
}