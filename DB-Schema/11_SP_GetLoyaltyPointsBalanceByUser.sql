-- =============================================================================
-- Loyalty Drip Mechanic — SP: SP_GetLoyaltyPointsBalanceByUser
-- Returns the three loyalty balances for a given customer.
-- Used by GET /v1/drip/balances/{customerId}
--
-- Parameters:
--   @CustomerId VARCHAR(10) — customer to query
--
-- Result (one row):
--   CrmLPTransactionBalance  — spendable loyalty points
--   DripPoolBalance          — accumulated points pending daily release
--   UnclaimedPoolBalance     — forfeited allotments never redeemed
-- =============================================================================
GO
CREATE  PROCEDURE [dbo].[SP_GetLoyaltyPointsBalanceByUser]
    @CustomerId VARCHAR(10) 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(lp.Balance, 0) AS CrmLPTransactionBalance,
        ISNULL(dp.Balance, 0) AS DripPoolBalance,
        ISNULL(up.Balance, 0) AS UnclaimedPoolBalance
    FROM  dbo.crmLPTransactionBalances lp
    LEFT  JOIN dbo.DripPool            dp ON dp.CustomerId = lp.CustomerID
    LEFT  JOIN dbo.UnclaimedPool       up ON up.CustomerId = lp.CustomerID
    WHERE lp.CustomerID = @CustomerId;
END
GO
