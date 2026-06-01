namespace LoyaltyPoints.Domain.Entities;

public enum ClaimStatus : byte
{
    Pending   = 0,
    Claimed   = 1,
    Forfeited = 2,
}

public class DailyClaimSnapshot
{
    public long SnapshotId { get; set; }
    public string CustomerId { get; set; } = null!;
    public DateOnly CycleDate { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
