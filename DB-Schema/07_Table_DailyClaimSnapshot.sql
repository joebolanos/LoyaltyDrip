-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: DailyClaimSnapshot
-- One record per customer — updated each cycle by the nightly job.
-- AllotmentAmount is frozen at job run time and used for both claim and forfeit.
-- Never recalculated against DripConfig after being set.
--
-- Status TINYINT:
--   0 = PENDING   — eligible, player can claim today
--   1 = CLAIMED   — player successfully claimed their allotment
--   2 = FORFEITED — nightly job ran before the player claimed
-- =============================================================================
CREATE TABLE DailyClaimSnapshot (
    SnapshotId      BIGINT          NOT NULL    IDENTITY(1,1),
    CustomerId      VARCHAR(10)     NOT NULL,
    CycleDate       DATE            NOT NULL,
    AllotmentAmount INT             NOT NULL    DEFAULT 0,
    Status          TINYINT         NOT NULL    DEFAULT 0,
    ClaimedAt       DATETIME2(3)    NULL,
    CreatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_DailyClaimSnapshot
        PRIMARY KEY (SnapshotId),

    -- One record per customer
    CONSTRAINT UQ_DailyClaimSnapshot_CustomerId
        UNIQUE (CustomerId),

    CONSTRAINT FK_DailyClaimSnapshot_Customer
        FOREIGN KEY (CustomerId)
        REFERENCES Customer(CustomerId),

    CONSTRAINT CK_DailyClaimSnapshot_AllotmentAmount
        CHECK (AllotmentAmount >= 0),

    CONSTRAINT CK_DailyClaimSnapshot_Status
        CHECK (Status IN (0, 1, 2)),

    -- ClaimedAt must be set only when Status = 1 (CLAIMED)
    CONSTRAINT CK_DailyClaimSnapshot_ClaimedAt
        CHECK (
            (Status = 1 AND ClaimedAt IS NOT NULL) OR
            (Status <> 1 AND ClaimedAt IS NULL)
        )
);
GO

-- Double-claim check (most frequent query)
CREATE INDEX IX_DailyClaimSnapshot_CustomerId
    ON DailyClaimSnapshot (CustomerId);
GO

-- Nightly job: find PENDING records from previous cycles
CREATE INDEX IX_DailyClaimSnapshot_Status_CycleDate
    ON DailyClaimSnapshot (Status, CycleDate)
    WHERE Status = 0;
GO
