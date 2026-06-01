using System.Data;
using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;

namespace LoyaltyPoints.Infrastructure.Repositories;

public sealed class DailyClaimSnapshotRepository : IDailyClaimSnapshotRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DailyClaimSnapshotRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<DailyClaimSnapshot?> GetByCustomerAndDateAsync(string customerId, DateOnly date)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<DailyClaimSnapshot>(
            @"SELECT SnapshotId, CustomerId, CycleDate, Status, ClaimedAt, CreatedAt
              FROM dbo.DailyClaimSnapshot
              WHERE CustomerId = @CustomerId
                AND CycleDate  = @CycleDate",
            new { CustomerId = customerId, CycleDate = date.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task<IEnumerable<DailyClaimSnapshot>> GetAllPendingBeforeDateAsync(DateOnly date)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<DailyClaimSnapshot>(
            @"SELECT SnapshotId, CustomerId, CycleDate, Status, ClaimedAt, CreatedAt
              FROM dbo.DailyClaimSnapshot
              WHERE Status    = @Status
                AND CycleDate < @CycleDate",
            new { Status = (byte)ClaimStatus.Pending, CycleDate = date.ToDateTime(TimeOnly.MinValue) });
    }

    public async Task InsertAsync(DailyClaimSnapshot snapshot, IDbTransaction? transaction = null)
    {
        const string sql =
            @"INSERT INTO dbo.DailyClaimSnapshot (CustomerId, CycleDate, Status, ClaimedAt, CreatedAt)
              VALUES (@CustomerId, @CycleDate, @Status, @ClaimedAt, @CreatedAt)";
        var param = new
        {
            snapshot.CustomerId,
            CycleDate = snapshot.CycleDate.ToDateTime(TimeOnly.MinValue),
            Status    = (byte)snapshot.Status,
            snapshot.ClaimedAt,
            snapshot.CreatedAt,
        };

        if (transaction is not null)
            await transaction.Connection!.ExecuteAsync(sql, param, transaction);
        else
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, param);
        }
    }

    public async Task UpdateStatusAsync(long snapshotId, ClaimStatus status, DateTime? claimedAt,
        IDbTransaction? transaction = null)
    {
        const string sql =
            @"UPDATE dbo.DailyClaimSnapshot
              SET Status    = @Status,
                  ClaimedAt = @ClaimedAt
              WHERE SnapshotId = @SnapshotId";
        var param = new { SnapshotId = snapshotId, Status = (byte)status, ClaimedAt = claimedAt };

        if (transaction is not null)
            await transaction.Connection!.ExecuteAsync(sql, param, transaction);
        else
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, param);
        }
    }
}
