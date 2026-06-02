namespace LoyaltyPoints.Application.Drip.Queries.GetDailyAllotmentPreview;

public class AllotmentPreviewDto
{
    public string CustomerId { get; set; } = null!;
    public int DripPoolBalance { get; set; }
    public int AllotmentPreview { get; set; }
    public decimal AllotmentPercent { get; set; }
    public int MinimumBalanceForClaim { get; set; }
    public bool AlreadyClaimed { get; set; }
    public bool IsEligible { get; set; }
}
