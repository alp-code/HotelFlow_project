using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Common;

namespace HotelFlow.Domain.Entities;

public class UserProfile : BaseEntity
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Phone { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    

    private UserProfile() { }

    public UserProfile(Guid userId, string firstName, string lastName, string phone)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
    }
    public void Update(string firstName, string lastName, string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty");

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
    }

}

