-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: DripConfig
-- Operational configuration for the drip mechanic.
-- Only one active record allowed at a time (IsActive = 1).
-- Each change inserts a new row — full history is preserved automatically.
-- ChangedBy records who applied each configuration change.
--
-- Fields:
--   ImmediateCreditRatio  — LP credited immediately to crmLPTransactionBalances per dollar wagered
--   DripPoolRatio         — LP allocated to DripPool per dollar wagered (independent of above)
--   DailyAllotmentPercent — % of DripPool offered as daily claimable allotment
--   MinimumBalanceForClaim — minimum DripPool balance required to be eligible for a daily allotment
-- =============================================================================
CREATE TABLE DripConfig (
    ConfigId                INT             NOT NULL    IDENTITY(1,1),
    ImmediateCreditRatio    DECIMAL(5,2)    NOT NULL,
    DripPoolRatio           DECIMAL(5,2)    NOT NULL,
    DailyAllotmentPercent   DECIMAL(5,2)    NOT NULL,
    MinimumBalanceForClaim  INT             NOT NULL,
    IsActive                BIT             NOT NULL    DEFAULT 1,
    ChangedBy               VARCHAR(50)     NOT NULL    DEFAULT 'system',
    CreatedAt               DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_DripConfig
        PRIMARY KEY (ConfigId),

    CONSTRAINT CK_DripConfig_ImmediateCreditRatio
        CHECK (ImmediateCreditRatio > 0),

    CONSTRAINT CK_DripConfig_DripPoolRatio
        CHECK (DripPoolRatio > 0),

    CONSTRAINT CK_DripConfig_DailyAllotmentPercent
        CHECK (DailyAllotmentPercent > 0 AND DailyAllotmentPercent <= 1),

    CONSTRAINT CK_DripConfig_MinimumBalanceForClaim
        CHECK (MinimumBalanceForClaim >= 0)
);
GO

-- Ensures only one active configuration record at a time
CREATE UNIQUE INDEX UQ_DripConfig_IsActive
    ON DripConfig (IsActive)
    WHERE IsActive = 1;
GO
