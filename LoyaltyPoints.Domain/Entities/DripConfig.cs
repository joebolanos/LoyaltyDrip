namespace LoyaltyPoints.Domain.Entities;

public class DripConfig
{
    public int ConfigId { get; set; }
    public decimal ImmediateCreditRatio { get; set; }
    public decimal DripPoolRatio { get; set; }
    public decimal DailyAllotmentPercent { get; set; }
    public int MinimumBalanceForClaim { get; set; }
    public bool IsActive { get; set; }
    public string ChangedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
