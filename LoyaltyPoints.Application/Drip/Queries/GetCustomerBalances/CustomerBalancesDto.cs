using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public class CustomerBalancesDto
{
    public float CrmLPTransactionBalance { get; set; }
    public int DripPoolBalance { get; set; }
    public int UnclaimedPoolBalance { get; set; }

    public static CustomerBalancesDto Map(
        CrmLPTransactionBalances lpBalance,
        DripPool? dripPool,
        UnclaimedPool? unclaimedPool) => new()
    {
        CrmLPTransactionBalance = lpBalance.Balance,
        DripPoolBalance         = dripPool?.Balance ?? 0,
        UnclaimedPoolBalance    = unclaimedPool?.Balance ?? 0,
    };
}
