using System.Data;
using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;

namespace LoyaltyPoints.Infrastructure.Repositories;

public sealed class LPBalanceRepository : ILPBalanceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LPBalanceRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<CrmLPTransactionBalances?> GetByCustomerIdAsync(string customerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<CrmLPTransactionBalances>(
            @"SELECT LPTransactionBalanceID, CustomerID, Balance, LifetimePoints, SeasonPoints,
                     LPTierID, LastTierUpdate, LastLifetimePointsUpdate, LastSeasonPoints, LastLPTierID, Comments
              FROM dbo.crmLPTransactionBalances
              WHERE CustomerID = @CustomerID",
            new { CustomerID = customerId });
    }

    public async Task AddPointsAsync(string customerId, int amount, IDbTransaction? transaction = null)
    {
        const string sql =
            @"UPDATE dbo.crmLPTransactionBalances
              SET Balance                  = Balance + @Amount,
                  LifetimePoints           = LifetimePoints + @Amount,
                  SeasonPoints             = SeasonPoints + @Amount,
                  LastLifetimePointsUpdate = GETUTCDATE()
              WHERE CustomerID = @CustomerID";
        var param = new { CustomerID = customerId, Amount = amount };

        if (transaction is not null)
            await transaction.Connection!.ExecuteAsync(sql, param, transaction);
        else
        {
            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, param);
        }
    }
}
