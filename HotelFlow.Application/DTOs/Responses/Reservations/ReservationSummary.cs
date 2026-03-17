using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Responses.Reservations;
public class ReservationSummary
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}