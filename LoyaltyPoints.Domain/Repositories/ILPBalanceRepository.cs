using System.Data;
using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Domain.Repositories;

/// <summary>Access to the existing dbo.crmLPTransactionBalances production table.</summary>
public interface ILPBalanceRepository
{
    Task<CrmLPTransactionBalances?> GetByCustomerIdAsync(string customerId);

    /// <summary>Atomically adds LP to Balance, LifetimePoints, and SeasonPoints.</summary>
    Task AddPointsAsync(string customerId, int amount, IDbTransaction? transaction = null);
}
