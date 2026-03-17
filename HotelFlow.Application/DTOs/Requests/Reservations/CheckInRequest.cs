using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Requests.Reservations;
public class CheckInRequest
{
    public Guid ReservationId { get; set; }
}