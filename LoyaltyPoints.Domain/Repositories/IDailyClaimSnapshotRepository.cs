using System.Data;
using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Domain.Repositories;

public interface IDailyClaimSnapshotRepository
{
    /// <summary>Returns the snapshot for a given customer and cycle date, or null if none exists.</summary>
    Task<DailyClaimSnapshot?> GetByCustomerAndDateAsync(string customerId, DateOnly date);

    /// <summary>Returns all PENDING snapshots with a CycleDate before <paramref name="date"/> — used by the nightly forfeit step.</summary>
    Task<IEnumerable<DailyClaimSnapshot>> GetAllPendingBeforeDateAsync(DateOnly date);

    Task InsertAsync(DailyClaimSnapshot snapshot, IDbTransaction? transaction = null);

    Task UpdateStatusAsync(long snapshotId, ClaimStatus status, DateTime? claimedAt,
        IDbTransaction? transaction = null);
}
