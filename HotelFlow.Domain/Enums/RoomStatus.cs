using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Domain.Enums;

public enum RoomStatus
{
    Available = 1,
    Occupied = 2,
    NeedsCleaning = 3,
    OutOfService = 4 
}