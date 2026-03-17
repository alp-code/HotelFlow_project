using HotelFlow.Domain.Common;
using HotelFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

// Domain/Entities/Reservation.cs
namespace HotelFlow.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid GuestId { get; private set; }
    public User Guest { get; private set; } = default!;

    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = default!;

    [Column("DateFrom")]
    public DateTime CheckInDate { get; private set; }
    [Column("DateTo")]
    public DateTime CheckOutDate { get; private set; }
    public int NumberOfGuests { get; private set; }

    public ReservationStatus Status { get; private set; }
    public string? SpecialRequests { get; private set; }

    public decimal TotalPrice { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? CheckedInAt { get; private set; }
    public DateTime? CheckedOutAt { get; private set; }

    public Guid? HousekeepingTaskId { get; private set; }
    public HousekeepingTask? HousekeepingTask { get; private set; }

    private Reservation() { }

    public Reservation(
        Guid guestId,
        Guid roomId,
        DateTime checkInDate,
        DateTime checkOutDate,
        int numberOfGuests,
        string? specialRequests,
        decimal totalPrice)
    {
        Id = Guid.NewGuid();
        GuestId = guestId;
        RoomId = roomId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        NumberOfGuests = numberOfGuests;
        Status = ReservationStatus.Confirmed;
        SpecialRequests = specialRequests;
        TotalPrice = totalPrice;
        IsPaid = false;
        CreatedAt = DateTime.UtcNow;

        Validate();
    }

    private void Validate()
    {
        if (CheckInDate < DateTime.Today)
            throw new Exception("Check-in date cannot be in the past");

        if (CheckOutDate <= CheckInDate)
            throw new Exception("Check-out date must be after check-in date");

        if (NumberOfGuests <= 0)
            throw new Exception("Number of guests must be positive");

        if (TotalPrice <= 0)
            throw new Exception("Total price must be positive");
    }

    public void CheckIn()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new Exception("Only confirmed reservations can be checked in");

        if (CheckInDate.Date > DateTime.Today)
            throw new Exception("Cannot check-in before check-in date");

        Status = ReservationStatus.CheckedIn;
        CheckedInAt = DateTime.UtcNow;
    }

    public void CheckOut()
    {
        if (Status != ReservationStatus.CheckedIn)
            throw new Exception("Only checked-in reservations can be checked out");

        Status = ReservationStatus.CheckedOut;
        CheckedOutAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.CheckedOut)
            throw new Exception("Cannot cancel already checked-out reservation");

        Status = ReservationStatus.Cancelled;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
    }

    public void NoShow()
    {
        Status = ReservationStatus.NoShow;
    }

    public void SetHousekeepingTask(Guid taskId)
    {
        HousekeepingTaskId = taskId;
    }
}