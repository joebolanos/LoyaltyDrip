using System.Data;

namespace LoyaltyPoints.Domain;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
