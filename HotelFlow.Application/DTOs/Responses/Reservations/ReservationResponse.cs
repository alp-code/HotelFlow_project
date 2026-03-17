namespace HotelFlow.Application.DTOs.Responses.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid GuestId { get; set; }
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPaid { get; set; }
    public int Nights => (CheckOutDate - CheckInDate).Days;
    public DateTime CreatedAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
}