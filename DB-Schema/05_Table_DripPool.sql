-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: DripPool
-- Accumulated LP balance pending daily release per customer.
-- Grows when wagers are processed by the nightly job.
-- Decreases when a daily allotment is claimed or forfeited.
-- Not directly redeemable — must be claimed first to convert to LP.
-- =============================================================================
CREATE TABLE DripPool (
    DripPoolId      BIGINT          NOT NULL    IDENTITY(1,1),
    CustomerId      VARCHAR(10)     NOT NULL,
    Balance         INT             NOT NULL    DEFAULT 0,
    CurrentBase     INT             NOT NULL    DEFAULT 0,
    LastRefillAt    DATETIME2(3)    NULL,
    UpdatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_DripPool
        PRIMARY KEY (DripPoolId),

    CONSTRAINT UQ_DripPool_CustomerId
        UNIQUE (CustomerId),

    CONSTRAINT FK_DripPool_Customer
        FOREIGN KEY (CustomerId)
        REFERENCES Customer(CustomerId),

    CONSTRAINT CK_DripPool_Balance
        CHECK (Balance >= 0),

    CONSTRAINT CK_DripPool_CurrentBase
        CHECK (CurrentBase >= 0)
);
GO

CREATE INDEX IX_DripPool_CustomerId
    ON DripPool (CustomerId);
GO

-- Filtered index for the nightly job — only customers with active balance
CREATE INDEX IX_DripPool_Balance
    ON DripPool (Balance)
    WHERE Balance > 0;
GO
