namespace LoyaltyPoints.Domain.Entities;

/// <summary>
/// Represents a player's unclaimed pool — the accumulated balance of daily
/// allotments that were not claimed within their 24-hour window.
/// Points here NEVER expire and are NEVER auto-credited to LoyaltyPoints.
/// They are only released via operator-configured special events or
/// re-engagement campaigns.
/// </summary>
public class UnclaimedPool
{
    /// <summary>Primary key — auto-incremented by SQL Server.</summary>
    public long UnclaimedPoolId { get; private set; }

    /// <summary>FK to Customer. One UnclaimedPool record per customer.</summary>
    public string CustomerId { get; private set; }

    /// <summary>
    /// Accumulated forfeited allotments in LP (always >= 0).
    /// Increases when a daily allotment is forfeited.
    /// Decreases when an operator-triggered event releases a portion to LoyaltyPoints.
    /// </summary>
    public int Balance { get; private set; }

    /// <summary>UTC timestamp of the last balance update.</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Required by Dapper for mapping query results.</summary>
    private UnclaimedPool() { }

    /// <summary>Creates a new UnclaimedPool record for a customer with zero balance.</summary>
    public UnclaimedPool(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        CustomerId = customerId;
        Balance    = 0;
        CreatedAt  = DateTime.UtcNow;
        UpdatedAt  = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Domain behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a forfeited allotment to the pool.
    /// Called by the 00:00 cron job (Cron Job 2) when a PENDING snapshot expires.
    /// </summary>
    /// <param name="amount">LP to add (must be > 0).</param>
    public void AddForfeited(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Forfeited amount must be greater than zero.");

        Balance   += amount;
        UpdatedAt  = DateTime.UtcNow;
    }

    /// <summary>
    /// Releases a portion of the pool to LoyaltyPoints as part of a
    /// special event or re-engagement campaign.
    /// Validates that the pool has sufficient balance.
    /// </summary>
    /// <param name="amount">LP to release (must be > 0 and <= Balance).</param>
    public void Release(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Release amount must be greater than zero.");

        if (amount > Balance)
            throw new InvalidOperationException(
                $"Cannot release {amount} LP from UnclaimedPool — current balance is {Balance}.");

        Balance   -= amount;
        UpdatedAt  = DateTime.UtcNow;
    }
}
