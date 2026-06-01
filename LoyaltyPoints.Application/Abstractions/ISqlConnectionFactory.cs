using Microsoft.Data.SqlClient;

namespace LoyaltyPoints.Application.Abstractions;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}
