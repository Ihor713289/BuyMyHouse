namespace BuyMyHouse.Application.Common.DTOs;

public record AuthResponse(string UserId, string Email, string FullName, string AccessToken, DateTimeOffset ExpiresAt);
