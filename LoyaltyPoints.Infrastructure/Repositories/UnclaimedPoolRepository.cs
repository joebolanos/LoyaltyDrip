using System.Data;
using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;

namespace LoyaltyPoints.Infrastructure.Repositories;

public sealed class UnclaimedPoolRepository : IUnclaimedPoolRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnclaimedPoolRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<UnclaimedPool?> GetByCustomerIdAsync(string customerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<UnclaimedPool>(
            @"SELECT UnclaimedPoolId, CustomerId, Balance, UpdatedAt, CreatedAt
              FROM dbo.UnclaimedPool
              WHERE CustomerId = @CustomerId",
            new { CustomerId = customerId });
    }

    public async Task UpsertAsync(UnclaimedPool pool)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.ExecuteAsync(
            @"MERGE dbo.UnclaimedPool AS target
              USING (SELECT @CustomerId AS CustomerId) AS source
                  ON target.CustomerId = source.CustomerId
              WHEN MATCHED THEN
                  UPDATE SET Balance   = @Balance,
                             UpdatedAt = @UpdatedAt
              WHEN NOT MATCHED THEN
                  INSERT (CustomerId, Balance, UpdatedAt, CreatedAt)
                  VALUES (@CustomerId, @Balance, @UpdatedAt, @CreatedAt);",
            new
            {
                pool.CustomerId,
                pool.Balance,
                pool.UpdatedAt,
                pool.CreatedAt,
            });
    }

    public async Task AddToBalanceAsync(string customerId, int amount, IDbTransaction? transaction = null)
    {
        const string sql =
            @"MERGE dbo.UnclaimedPool AS target
              USING (SELECT @CustomerId AS CustomerId) AS source
                  ON target.CustomerId = source.CustomerId
              WHEN MATCHED THEN
                  UPDATE SET Balance   = Balance + @Amount,
                             UpdatedAt = GETUTCDATE()
              WHEN NOT MATCHED THEN
                  INSERT (CustomerId, Balance, UpdatedAt, CreatedAt)
                  VALUES (@CustomerId, @Amount, GETUTCDATE(), GETUTCDATE());";
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
