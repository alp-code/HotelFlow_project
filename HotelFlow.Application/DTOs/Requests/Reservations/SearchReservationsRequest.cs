
namespace HotelFlow.Application.DTOs.Requests.Reservations;

public class SearchReservationsRequest
{
    public string? GuestEmail { get; set; }
    public string? GuestName { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? RoomNumber { get; set; }
}