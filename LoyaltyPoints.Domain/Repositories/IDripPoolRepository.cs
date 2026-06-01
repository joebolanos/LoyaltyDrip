using System.Data;
using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Domain.Repositories;

public interface IDripPoolRepository
{
    Task<DripPool?> GetByCustomerIdAsync(string customerId);

    /// <summary>Returns all DripPool records with a positive balance — used by the nightly job.</summary>
    Task<IEnumerable<DripPool>> GetAllWithBalanceAsync();

    /// <summary>INSERT or UPDATE based on CustomerId.</summary>
    Task UpsertAsync(DripPool dripPool);

    /// <summary>Atomically subtracts <paramref name="amount"/> from Balance.</summary>
    Task DeductBalanceAsync(string customerId, int amount, IDbTransaction? transaction = null);
}
