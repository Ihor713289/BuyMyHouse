using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Application.Common.Services;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(User user);
}
