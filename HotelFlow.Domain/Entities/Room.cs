using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Common;
using HotelFlow.Domain.Enums;

namespace HotelFlow.Domain.Entities;

public class Room : BaseEntity
{
    public string RoomNumber { get; private set; } = default!;
    public RoomStatus Status { get; private set; }

    public Guid RoomTypeId { get; private set; }
    public RoomType RoomType { get; private set; } = default!;
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<HousekeepingTask> HousekeepingTasks { get; private set; } = new List<HousekeepingTask>();

    private Room() { }

    public Room(string roomNumber, Guid roomTypeId)
    {
        RoomNumber = roomNumber;
        RoomTypeId = roomTypeId;
        Status = RoomStatus.Available;
    }
    public void Update(string roomNumber, RoomStatus status)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new ArgumentException("Room number cannot be empty");

        RoomNumber = roomNumber;
        Status = status;
    }
    public void Occupied()
    {
        if (Status != RoomStatus.Available)
            throw new Exception($"Cannot reserve room with status: {Status}");

        Status = RoomStatus.Occupied;
    }

    public void CheckIn()
    {
        if (Status != RoomStatus.Available)
            throw new Exception($"Cannot check-in to room with status: {Status}");

        Status = RoomStatus.Occupied;
    }

    public void CheckOut()
    {
        if (Status != RoomStatus.Occupied && Status != RoomStatus.Available)
            throw new Exception($"Cannot check-out from room with status: {Status}");

        Status = RoomStatus.NeedsCleaning;
    }

    public void MarkAsCleaned()
    {
        if (Status != RoomStatus.NeedsCleaning && Status != RoomStatus.OutOfService && Status != RoomStatus.Cleaning)
            throw new Exception($"Cannot clean room with status: {Status}");

        Status = RoomStatus.Available;
    }

    public void RoomOutOfService()
    {
        Status = RoomStatus.OutOfService;
    }

    public void ChangeStatus(RoomStatus newStatus)
    {
        Status = newStatus;
    }
}
