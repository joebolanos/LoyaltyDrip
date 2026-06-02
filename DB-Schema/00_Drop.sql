-- =============================================================================
-- Loyalty Drip Mechanic — DROP
-- Order: SPs first, then child tables, then parent tables.
-- Run this before re-creating the schema from scratch.
-- =============================================================================

-- Stored Procedures
IF OBJECT_ID('sp_ProcessNightlyDripJob',         'P') IS NOT NULL DROP PROCEDURE sp_ProcessNightlyDripJob;
IF OBJECT_ID('sp_ClaimDailyAllotment',           'P') IS NOT NULL DROP PROCEDURE sp_ClaimDailyAllotment;
IF OBJECT_ID('sp_GetDailyAllotmentPreview',      'P') IS NOT NULL DROP PROCEDURE sp_GetDailyAllotmentPreview;
IF OBJECT_ID('SP_GetLoyaltyPointsBalanceByUser', 'P') IS NOT NULL DROP PROCEDURE SP_GetLoyaltyPointsBalanceByUser;
GO

-- Child tables (FK dependency on Customer)
IF OBJECT_ID('DailyClaimSnapshot',       'U') IS NOT NULL DROP TABLE DailyClaimSnapshot;
IF OBJECT_ID('UnclaimedPool',            'U') IS NOT NULL DROP TABLE UnclaimedPool;
IF OBJECT_ID('DripPool',                 'U') IS NOT NULL DROP TABLE DripPool;
IF OBJECT_ID('crmLPTransactions',        'U') IS NOT NULL DROP TABLE crmLPTransactions;
IF OBJECT_ID('DripConfig',               'U') IS NOT NULL DROP TABLE DripConfig;
IF OBJECT_ID('crmLPTransactionBalances', 'U') IS NOT NULL DROP TABLE crmLPTransactionBalances;
GO

-- Root table
IF OBJECT_ID('Customer', 'U') IS NOT NULL DROP TABLE Customer;
GO
