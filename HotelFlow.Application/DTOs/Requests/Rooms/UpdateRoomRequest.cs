using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Requests.Rooms;

public class UpdateRoomRequest
{
    public string RoomNumber { get; set; } = default!;
    public string Status { get; set; } = default!;
}
