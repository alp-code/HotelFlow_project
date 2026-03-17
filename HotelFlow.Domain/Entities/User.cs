using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Common;
using HotelFlow.Domain.Enums;

namespace HotelFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public void SoftDelete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
    public UserProfile? Profile { get; private set; }

    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<HousekeepingTask> HousekeepingTasks { get; private set; } = new List<HousekeepingTask>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    private User() { } // EF Core

    public User(string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }
    public UserProfile CreateProfile(string firstName, string lastName, string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty", nameof(phone));

        Profile = new UserProfile(Id, firstName, lastName, phone);
        return Profile;
    }
    public RefreshToken CreateRefreshToken(string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken(token, expiresAt, this);
        return refreshToken;
    }
    public void ChangeRole(UserRole role)
    {
        Role = role;
    }

}
