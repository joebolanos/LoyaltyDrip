-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: UnclaimedPool
-- Accumulated forfeited allotments per customer.
-- Points here NEVER expire — they accumulate indefinitely.
-- Only released via operator-configured special events or re-engagement campaigns.
-- =============================================================================
CREATE TABLE UnclaimedPool (
    UnclaimedPoolId BIGINT          NOT NULL    IDENTITY(1,1),
    CustomerId      VARCHAR(10)     NOT NULL,
    Balance         INT             NOT NULL    DEFAULT 0,
    UpdatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedAt       DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_UnclaimedPool
        PRIMARY KEY (UnclaimedPoolId),

    CONSTRAINT UQ_UnclaimedPool_CustomerId
        UNIQUE (CustomerId),

    CONSTRAINT FK_UnclaimedPool_Customer
        FOREIGN KEY (CustomerId)
        REFERENCES Customer(CustomerId),

    CONSTRAINT CK_UnclaimedPool_Balance
        CHECK (Balance >= 0)
);
GO

CREATE INDEX IX_UnclaimedPool_CustomerId
    ON UnclaimedPool (CustomerId);
GO
