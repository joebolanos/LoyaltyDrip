-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: crmLPTransactions
-- Wagers pending processing by the nightly job.
-- Processed = 0 means the wager has not yet been credited to LP or DripPool.
-- =============================================================================
CREATE TABLE crmLPTransactions (
    TransactionId   BIGINT          NOT NULL    IDENTITY(1,1),
    CustomerId      VARCHAR(10)     NOT NULL,
    WagerAmount     INT             NOT NULL,
    Processed       BIT             NOT NULL    DEFAULT 0,
    ProcessedAt     DATETIME2(3)    NULL,
    CreatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_crmLPTransactions
        PRIMARY KEY (TransactionId),

    CONSTRAINT FK_crmLPTransactions_Customer
        FOREIGN KEY (CustomerId)
        REFERENCES Customer(CustomerId),

    CONSTRAINT CK_crmLPTransactions_WagerAmount
        CHECK (WagerAmount > 0)
);
GO

-- Filtered index for the nightly job — only unprocessed wagers
CREATE INDEX IX_crmLPTransactions_Processed
    ON crmLPTransactions (Processed, CustomerId)
    WHERE Processed = 0;
GO
