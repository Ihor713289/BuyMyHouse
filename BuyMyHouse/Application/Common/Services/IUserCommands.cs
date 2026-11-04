using BuyMyHouse.Domain.Entities;

namespace BuyMyHouse.Application.Common.Services;

public interface IUserCommands
{
    // Returns false when a user with the same email already exists.
    Task<bool> TryCreateUser(User user, CancellationToken cancellationToken = default);
}
