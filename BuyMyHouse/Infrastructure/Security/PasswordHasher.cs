using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BuyMyHouse.Infrastructure.Security;

// Wraps ASP.NET Core Identity's hasher (salted PBKDF2-HMAC-SHA512, versioned format).
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword) =>
        _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword) != PasswordVerificationResult.Failed;
}
