using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Common;

namespace HotelFlow.Domain.Entities;

public class RoomType : BaseEntity
{
    public string Name { get; private set; } = default!;
    public decimal PricePerNight { get; private set; }
    public int MaxGuests { get; private set; }
    public string? Description { get; private set; }

    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    private RoomType() { }

    public RoomType(string name, decimal pricePerNight, int maxGuests, string? description = null)
    {
        Name = name;
        PricePerNight = pricePerNight;
        MaxGuests = maxGuests;
        Description = description;

        Validate();
    }
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Room type name is required", nameof(Name));

        if (PricePerNight <= 0)
            throw new ArgumentException("Price per night must be greater than zero", nameof(PricePerNight));

        if (MaxGuests <= 0)
            throw new ArgumentException("Max guests must be greater than zero", nameof(MaxGuests));
    }

    public void UpdatePrice(decimal newPrice)
    {
        PricePerNight = newPrice;
    }
    public void UpdateMaxGuests(int maxGuests)
    {
        if (maxGuests <= 0)
            throw new ArgumentException("Max guests must be greater than zero", nameof(maxGuests));

        MaxGuests = maxGuests;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateDetails(string name, decimal pricePerNight, int maxGuests, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name is required", nameof(name));

        if (pricePerNight <= 0)
            throw new ArgumentException("Price per night must be greater than zero", nameof(pricePerNight));

        if (maxGuests <= 0)
            throw new ArgumentException("Max guests must be greater than zero", nameof(maxGuests));

        Name = name;
        PricePerNight = pricePerNight;
        MaxGuests = maxGuests;
        Description = description;
    }
}
