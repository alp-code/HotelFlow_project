using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Responses.Reservations;
public class AvailableRoomResponse
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public string Description { get; set; } = string.Empty;
}
