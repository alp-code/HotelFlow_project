using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Domain.Enums;

public enum ReservationStatus
{
    Confirmed = 1,
    CheckedIn = 2,
    CheckedOut = 3,
    Cancelled = 4,
    NoShow = 5
}