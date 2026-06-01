using System.Data;
using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;

namespace LoyaltyPoints.Infrastructure.Repositories;

public sealed class DripPoolRepository : IDripPoolRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DripPoolRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<DripPool?> GetByCustomerIdAsync(string customerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<DripPool>(
            @"SELECT DripPoolId, CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt
              FROM dbo.DripPool
              WHERE CustomerId = @CustomerId",
            new { CustomerId = customerId });
    }

    public async Task<IEnumerable<DripPool>> GetAllWithBalanceAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<DripPool>(
            @"SELECT DripPoolId, CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt
              FROM dbo.DripPool
              WHERE Balance > 0");
    }

    public async Task UpsertAsync(DripPool dripPool)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            @"MERGE dbo.DripPool AS target
              USING (SELECT @CustomerId AS CustomerId) AS source
                  ON target.CustomerId = source.CustomerId
              WHEN MATCHED THEN
                  UPDATE SET Balance      = @Balance,
                             CurrentBase  = @CurrentBase,
                             LastRefillAt = @LastRefillAt,
                             UpdatedAt    = @UpdatedAt
              WHEN NOT MATCHED THEN
                  INSERT (CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt)
                  VALUES (@CustomerId, @Balance, @CurrentBase, @LastRefillAt, @UpdatedAt, @CreatedAt);",
            new
            {
                dripPool.CustomerId,
                dripPool.Balance,
                dripPool.CurrentBase,
                dripPool.LastRefillAt,
                dripPool.UpdatedAt,
                dripPool.CreatedAt,
            });
    }

    public async Task DeductBalanceAsync(string customerId, int amount, IDbTransaction? transaction = null)
    {
        const string sql =
            @"UPDATE dbo.DripPool
              SET Balance   = Balance - @Amount,
                  UpdatedAt = GETUTCDATE()
              WHERE CustomerId = @CustomerId";
        var param = new { CustomerId = customerId, Amount = amount };

        if (transaction is not null)
            await transaction.Connection!.ExecuteAsync(sql, param, transaction);
        else
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, param);
        }
    }
}
