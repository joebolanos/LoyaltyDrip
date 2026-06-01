namespace LoyaltyPoints.Domain.Entities;

/// <summary>
/// Represents the existing loyalty points balance record for a customer.
/// Maps to the production table dbo.crmLPTransactionBalances.
///
/// This entity is the ONLY source of spendable Loyalty Points (LP).
/// The DripPool and UnclaimedPool are non-redeemable until converted
/// into this balance via a daily claim or special event.
///
/// NOTE: Balance, LifetimePoints and SeasonPoints are stored as FLOAT
/// in the database (matching the production schema). All drip mechanic
/// calculations use INT — rounding is applied before writing back here.
/// </summary>
public class CrmLPTransactionBalances
{
    /// <summary>Primary key — auto-incremented by SQL Server.</summary>
    public int LPTransactionBalanceID { get; private set; }

    /// <summary>FK to Customer. One record per customer.</summary>
    public string CustomerID { get; private set; }

    /// <summary>
    /// Current spendable LP balance.
    /// Increases when a daily drip allotment is claimed or a wager is graded.
    /// Decreases when LP are redeemed (free plays, loyalty mall).
    /// </summary>
    public float Balance { get; private set; }

    /// <summary>
    /// Cumulative LP earned by the customer across all time.
    /// Never decreases — used for VIP tier calculations.
    /// </summary>
    public float LifetimePoints { get; private set; }

    /// <summary>LP earned within the current season window.</summary>
    public float SeasonPoints { get; private set; }

    /// <summary>Current VIP tier ID assigned to the customer.</summary>
    public int LPTierID { get; private set; }

    /// <summary>UTC timestamp of the last tier change.</summary>
    public DateTime? LastTierUpdate { get; private set; }

    /// <summary>UTC timestamp of the last LifetimePoints update.</summary>
    public DateTime? LastLifetimePointsUpdate { get; private set; }

    /// <summary>SeasonPoints balance at the start of the current season (snapshot).</summary>
    public float LastSeasonPoints { get; private set; }

    /// <summary>VIP tier ID held before the most recent tier change.</summary>
    public int LastLPTierID { get; private set; }

    /// <summary>Free-text operational notes. Nullable.</summary>
    public string? Comments { get; private set; }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>Required by Dapper for mapping query results.</summary>
    private CrmLPTransactionBalances() { }

    /// <summary>Creates a new LP balance record for a customer starting at zero.</summary>
    public CrmLPTransactionBalances(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerID is required.", nameof(customerId));

        CustomerID   = customerId;
        Balance      = 0;
        LifetimePoints           = 0;
        SeasonPoints             = 0;
        LPTierID     = 0;
        LastSeasonPoints         = 0;
        LastLPTierID = 0;
    }

    // -------------------------------------------------------------------------
    // Domain behaviour
    // -------------------------------------------------------------------------

    /// <summary>
    /// Credits LP to the spendable balance, LifetimePoints and SeasonPoints.
    /// Called when a wager is graded (immediate credit) or when a daily
    /// drip allotment is claimed.
    /// </summary>
    /// <param name="amount">LP to credit (must be > 0).</param>
    public void CreditPoints(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be greater than zero.");

        Balance        += amount;
        LifetimePoints += amount;
        SeasonPoints   += amount;
        LastLifetimePointsUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deducts LP from the spendable balance for a redemption (free play or loyalty mall).
    /// Does NOT affect LifetimePoints or SeasonPoints — those only ever increase.
    /// </summary>
    /// <param name="amount">LP to deduct (must be > 0 and <= Balance).</param>
    public void DeductPoints(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Deduction amount must be greater than zero.");

        if (amount > Balance)
            throw new InvalidOperationException(
                $"Cannot deduct {amount} LP — current spendable balance is {Balance}.");

        Balance -= amount;
    }

    /// <summary>
    /// Updates the customer's VIP tier.
    /// Stores the previous tier in LastLPTierID before applying the new one.
    /// </summary>
    /// <param name="newTierId">The new tier ID to assign.</param>
    public void UpdateTier(int newTierId)
    {
        LastLPTierID  = LPTierID;
        LPTierID      = newTierId;
        LastTierUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Snapshots the current SeasonPoints at the start of a new season.
    /// Called by the season-rollover process (out of scope for drip mechanic).
    /// </summary>
    public void RolloverSeason()
    {
        LastSeasonPoints = SeasonPoints;
        SeasonPoints     = 0;
    }

    /// <summary>
    /// Appends an operational note to the Comments field.
    /// </summary>
    public void AddComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment)) return;
        Comments = string.IsNullOrWhiteSpace(Comments)
            ? comment
            : $"{Comments} | {comment}";
    }
}
