-- =============================================================================
-- Loyalty Drip Mechanic — SEED DATA
-- For development and testing only. Do NOT run in production.
-- =============================================================================

-- Customers
INSERT INTO Customer (CustomerId, FullName, Email) VALUES
    ('CUST000001', 'John Doe',      'john.doe@test.com'),
    ('CUST000002', 'Jane Smith',    'jane.smith@test.com'),
    ('CUST000003', 'Carlos Mendez', 'carlos.mendez@test.com');
GO

-- LP balances
INSERT INTO crmLPTransactionBalances (CustomerID, Balance, LifetimePoints, SeasonPoints, LPTierID, LastLPTierID) VALUES
    ('CUST000001', 1000, 5000, 200, 1, 1),
    ('CUST000002', 500,  2000, 100, 1, 1),
    ('CUST000003', 250,  800,  50,  0, 0);
GO

-- Active configuration:
--   ImmediateCreditRatio  = 1.00 (1 LP per $1 wagered → crmLPTransactionBalances)
--   DripPoolRatio         = 1.00 (1 LP per $1 wagered → DripPool)
--   DailyAllotmentPercent = 0.20 (20% of DripPool offered daily)
--   MinimumBalanceForClaim = 100 (minimum 100 LP in DripPool to be eligible)
INSERT INTO DripConfig (ImmediateCreditRatio, DripPoolRatio, DailyAllotmentPercent, MinimumBalanceForClaim, ChangedBy) VALUES
    (1.00, 1.00, 0.20, 100, 'system');
GO

-- Unprocessed wagers for nightly job simulation
INSERT INTO crmLPTransactions (CustomerId, WagerAmount) VALUES
    ('CUST000001', 100),
    ('CUST000001', 250),
    ('CUST000001', 300),
    ('CUST000002', 500),
    ('CUST000002', 150),
    ('CUST000003', 75),
    ('CUST000003', 15);
GO
