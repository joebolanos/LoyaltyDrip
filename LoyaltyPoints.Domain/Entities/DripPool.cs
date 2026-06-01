namespace LoyaltyPoints.Domain.Entities;

public class DripPool
{
    public long DripPoolId { get; set; }
    public string CustomerId { get; set; } = null!;
    public int Balance { get; set; }
    public int CurrentBase { get; set; }
    public DateTime? LastRefillAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
