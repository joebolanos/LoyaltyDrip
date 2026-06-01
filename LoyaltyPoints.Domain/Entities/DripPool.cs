namespace LoyaltyPoints.Domain.Entities;

/// <summary>
/// Represents a player's drip pool — the accumulated balance of loyalty points
/// pending daily release. Points are added at wager grade time (not placement).
/// The pool is never directly redeemable; players must claim their daily allotment
/// via DailyClaimSnapshot to convert points into spendable LoyaltyPoints.
/// </summary>
public class DripPool
{
    /// <summary>Primary key — auto-incremented by SQL Server.</summary>
    public long DripPoolId { get; private set; }

    /// <summary>FK to Customer. One DripPool record per customer.</summary>
    public string CustomerId { get; private set; }

    /// <summary>
    /// Current balance in the pool (integer LP, always >= 0).
    /// Increases on every wager grade. Decreases when a daily allotment
    /// is claimed or forfeited.
    /// </summary>
    public int Balance { get; private set; }

    /// <summary>
    /// The balance at the moment of the last 00:00 snapshot.
    /// Used by the daily cron job to calculate the allotment for the current cycle.
    /// Gradeos during an active cycle update Balance but NOT CurrentBase —
    /// the new base takes effect in the next snapshot.
    /// </summary>
    public int CurrentBase { get; private set; }

    /// <summary>UTC timestamp of the last wager grade that added points to this pool.</summary>
    public DateTime? LastRefillAt { get; private set; }

    /// <summary>UTC timestamp of the last balance update.</summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Required by Dapper for mapping query results.</summary>
    private DripPool() { }

    /// <summary>Creates a new DripPool record for a customer with zero balance.</summary>
    public DripPool(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        CustomerId  = customerId;
        Balance     = 0;
        CurrentBase = 0;
        CreatedAt   = DateTime.UtcNow;
        UpdatedAt   = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Domain behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds points to the pool from a graded wager.
    /// Resets LastRefillAt and marks the pool as updated.
    /// Does NOT update CurrentBase — that is done exclusively by the snapshot cron job.
    /// </summary>
    /// <param name="points">LP to add (must be > 0).</param>
    public void Refill(int points)
    {
        if (points <= 0)
            throw new ArgumentOutOfRangeException(nameof(points), "Refill amount must be greater than zero.");

        Balance      += points;
        LastRefillAt  = DateTime.UtcNow;
        UpdatedAt     = DateTime.UtcNow;
    }

    /// <summary>
    /// Deducts the allotment when a daily claim is processed (claimed or forfeited).
    /// Validates that the pool has sufficient balance.
    /// </summary>
    /// <param name="amount">LP to deduct (must be > 0 and <= Balance).</param>
    public void DeductAllotment(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Allotment amount must be greater than zero.");

        if (amount > Balance)
            throw new InvalidOperationException(
                $"Cannot deduct {amount} LP from DripPool — current balance is {Balance}.");

        Balance   -= amount;
        UpdatedAt  = DateTime.UtcNow;
    }

    /// <summary>
    /// Freezes the current Balance as the new CurrentBase for the next cycle.
    /// Called exclusively by the 00:00 snapshot cron job before creating DailyClaimSnapshot records.
    /// </summary>
    public void FreezeBaseForCycle()
    {
        CurrentBase = Balance;
        UpdatedAt   = DateTime.UtcNow;
    }
}
