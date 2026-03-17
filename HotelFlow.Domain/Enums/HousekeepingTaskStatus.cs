using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Domain.Enums;

public enum HousekeepingTaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5
}

