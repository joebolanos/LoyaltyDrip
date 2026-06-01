namespace LoyaltyPoints.Domain.Entities;

public class CrmLPTransactionBalances
{
    public int LPTransactionBalanceID { get; set; }
    public string CustomerID { get; set; } = null!;
    public float Balance { get; set; }
    public float LifetimePoints { get; set; }
    public float SeasonPoints { get; set; }
    public int LPTierID { get; set; }
    public DateTime? LastTierUpdate { get; set; }
    public DateTime? LastLifetimePointsUpdate { get; set; }
    public float LastSeasonPoints { get; set; }
    public int LastLPTierID { get; set; }
    public string? Comments { get; set; }
}
