namespace LoyaltyPoints.Domain.Entities;

/// <summary>
/// Status of a daily allotment within its claim window.
/// Stored as TINYINT in SQL Server.
/// </summary>
public enum ClaimStatus : byte
{
    /// <summary>Claim window is open — player has not yet claimed today.</summary>
    Pending   = 0,

    /// <summary>Player successfully claimed their allotment today.</summary>
    Claimed   = 1,

    /// <summary>2am Job ran before the player claimed — allotment moved to UnclaimedPool.</summary>
    Forfeited = 2,
}

/// <summary>
/// Tracks whether a player has claimed their daily drip allotment.
/// One record per CustomerId per day, created ONLY when a claim is made.
///
/// Responsibilities:
///   1. Prevents double-claiming: if a CLAIMED record exists for today, block the claim.
///   2. Source of truth for the nightly job: PENDING records from previous days get forfeited.
///
/// The allotment amount is NOT stored here — it is calculated on-the-fly as
/// DripPool.Balance x configured percentage at the moment the player claims.
/// </summary>
public class DailyClaimSnapshot
{
    /// <summary>Primary key — auto-incremented by SQL Server.</summary>
    public long SnapshotId { get; private set; }

    /// <summary>FK to Customer.</summary>
    public string CustomerId { get; private set; }

    /// <summary>
    /// UTC date of the cycle this snapshot belongs to.
    /// Stored as DATE (no time component) to support the unique constraint CustomerId + CycleDate.
    /// </summary>
    public DateOnly CycleDate { get; private set; }

    /// <summary>Current status of the claim (Pending → Claimed or Forfeited).</summary>
    public ClaimStatus Status { get; private set; }

    /// <summary>
    /// UTC timestamp when the player tapped Claim.
    /// Null if Status is Forfeited (player never claimed).
    /// </summary>
    public DateTime? ClaimedAt { get; private set; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Required by Dapper for mapping query results.</summary>
    private DailyClaimSnapshot() { }

    /// <summary>
    /// Creates a new snapshot record when a player successfully claims their allotment.
    /// This is the ONLY moment a snapshot is created — not at the start of the cycle.
    /// </summary>
    /// <param name="customerId">The player's identifier.</param>
    /// <param name="cycleDate">Today's UTC date.</param>
    public DailyClaimSnapshot(string customerId, DateOnly cycleDate)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        CustomerId = customerId;
        CycleDate  = cycleDate;
        Status     = ClaimStatus.Claimed;
        ClaimedAt  = DateTime.UtcNow;
        CreatedAt  = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Domain behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Marks this record as forfeited when the 2am nightly job runs
    /// and finds it still in Pending status from a previous day.
    /// Can only transition Pending → Forfeited.
    /// </summary>
    /// <exception cref="InvalidOperationException">If Status is not Pending.</exception>
    public void MarkAsForfeited()
    {
        if (Status != ClaimStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot forfeit snapshot {SnapshotId} — current status is {Status}.");

        Status = ClaimStatus.Forfeited;
    }

    // -------------------------------------------------------------------------
    // Query helpers
    // -------------------------------------------------------------------------

    /// <summary>Returns true if the player already claimed today.</summary>
    public bool IsClaimed => Status == ClaimStatus.Claimed;

    /// <summary>Returns true if the allotment was lost to the nightly job.</summary>
    public bool IsForfeited => Status == ClaimStatus.Forfeited;
}
