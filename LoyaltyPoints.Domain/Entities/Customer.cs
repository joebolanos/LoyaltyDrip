namespace LoyaltyPoints.Domain.Entities;

public class Customer
{
    public string CustomerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
