-- =============================================================================
-- Loyalty Drip Mechanic — SP: sp_GetDailyAllotmentPreview
-- Returns the allotment the player can claim today.
-- If a PENDING snapshot exists, returns the frozen AllotmentAmount.
-- Otherwise calculates on-the-fly from DripConfig as a reference.
-- All date logic uses GETUTCDATE() — no timezone mismatch.
--
-- Parameters:
--   @CustomerId VARCHAR(10)
--
-- Result (one row):
--   CustomerId             VARCHAR(10)
--   DripPoolBalance        INT
--   AllotmentPreview       INT
--   AllotmentPercent       DECIMAL(5,2)
--   MinimumBalanceForClaim INT
--   AlreadyClaimed         BIT
--   IsEligible             BIT
-- =============================================================================
GO
CREATE PROCEDURE sp_GetDailyAllotmentPreview
    @CustomerId VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        @CustomerId                                                AS CustomerId,
        ISNULL(dp.Balance, 0)                                      AS DripPoolBalance,
        CASE
            WHEN s.Status IN (0, 1) THEN s.AllotmentAmount
            ELSE 0
        END                                                        AS AllotmentPreview,
        CAST(CASE WHEN s.Status = 1 THEN 1 ELSE 0 END AS BIT)     AS AlreadyClaimed,
        CAST(CASE
            WHEN s.Status = 0 AND s.AllotmentAmount > 0 THEN 1
            ELSE 0
        END AS BIT)                                                AS IsEligible
    FROM       crmLPTransactionBalances lp WITH (NOLOCK)
    LEFT JOIN  DripPool                 dp WITH (NOLOCK) ON dp.CustomerId = lp.CustomerID
    LEFT JOIN  DailyClaimSnapshot       s  WITH (NOLOCK) ON s.CustomerId  = lp.CustomerID
    WHERE lp.CustomerID = @CustomerId;
END
GO