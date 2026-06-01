namespace LoyaltyPoints.Domain.Entities;

public class UnclaimedPool
{
    public long UnclaimedPoolId { get; set; }
    public string CustomerId { get; set; } = null!;
    public int Balance { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
