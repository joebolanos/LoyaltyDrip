using Dapper;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;

namespace LoyaltyPoints.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerRepository(IDbConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public async Task<Customer?> GetByIdAsync(string customerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Customer>(
            @"SELECT CustomerId, FullName, Email, IsActive, CreatedAt
              FROM dbo.Customer
              WHERE CustomerId = @CustomerId",
            new { CustomerId = customerId });
    }
}
