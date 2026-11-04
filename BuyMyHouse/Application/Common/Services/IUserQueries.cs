using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Application.Common.Services;

public interface IUserQueries
{
    Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default);
}
