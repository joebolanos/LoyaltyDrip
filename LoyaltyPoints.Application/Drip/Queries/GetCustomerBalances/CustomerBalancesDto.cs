namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public class CustomerBalancesDto
{
    public float CrmLPTransactionBalance { get; set; }
    public int DripPoolBalance { get; set; }
    public int UnclaimedPoolBalance { get; set; }
}
