using System.Data;
using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Domain.Repositories;

public interface IUnclaimedPoolRepository
{
    Task<UnclaimedPool?> GetByCustomerIdAsync(string customerId);

    /// <summary>INSERT or UPDATE based on CustomerId.</summary>
    Task UpsertAsync(UnclaimedPool pool);

    /// <summary>Atomically adds <paramref name="amount"/> to Balance. Creates the record if it does not exist.</summary>
    Task AddToBalanceAsync(string customerId, int amount, IDbTransaction? transaction = null);
}
