using LoyaltyPoints.Domain.Entities;

namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public class CustomerBalancesDto
{
    public int LpBalance { get; set; }
    public int DripPoolBalance { get; set; }
    public int UnclaimedBalance { get; set; }

    public static CustomerBalancesDto Map(
        CrmLPTransactionBalances lpBalance,
        DripPool? dripPool,
        UnclaimedPool? unclaimedPool) => new()
    {
        LpBalance        = (int)lpBalance.Balance,
        DripPoolBalance  = dripPool?.Balance ?? 0,
        UnclaimedBalance = unclaimedPool?.Balance ?? 0,
    };
}
