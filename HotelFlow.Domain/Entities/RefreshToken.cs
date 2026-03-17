using HotelFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;

    private RefreshToken() { } // EF

    public RefreshToken(string token, DateTime expiresAt, User user)
    {
        Token = token;
        ExpiresAt = expiresAt;
        User = user;
        UserId = user.Id;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

}
