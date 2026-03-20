namespace HotelFlow.Application.DTOs.Responses.Reservations;

public class ReservationSummary
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Nights => (CheckOutDate - CheckInDate).Days;
}