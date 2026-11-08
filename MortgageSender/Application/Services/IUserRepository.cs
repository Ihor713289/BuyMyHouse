using BuyMyHouse.Domain.Entities;

namespace MortgageSender.Application.Services;

public interface IUserRepository
{
    // The user id is the JWT "sub" claim, which is also the MortgageApplication's PartitionKey.
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken);
}
