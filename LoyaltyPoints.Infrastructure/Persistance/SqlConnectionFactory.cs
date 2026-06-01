using LoyaltyPoints.Application.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LoyaltyPoints.Infrastructure.Persistance;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public SqlConnection CreateConnection()
        => new(configuration.GetConnectionString("Default"));
}
