-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: crmLPTransactionBalances
-- Existing production table — spendable loyalty points balance per customer.
-- This is the ONLY redeemable balance (free plays, loyalty mall).
-- =============================================================================
CREATE TABLE crmLPTransactionBalances (
    LPTransactionBalanceID      INT             NOT NULL    IDENTITY(1,1),
    CustomerID                  VARCHAR(10)     NOT NULL,
    Balance                     FLOAT           NOT NULL    DEFAULT 0,
    LifetimePoints              FLOAT           NOT NULL    DEFAULT 0,
    SeasonPoints                FLOAT           NOT NULL    DEFAULT 0,
    LPTierID                    INT             NOT NULL    DEFAULT 0,
    LastTierUpdate              DATETIME        NULL,
    LastLifetimePointsUpdate    DATETIME        NULL,
    LastSeasonPoints            FLOAT           NOT NULL    DEFAULT 0,
    LastLPTierID                INT             NOT NULL    DEFAULT 0,
    Comments                    VARCHAR(MAX)    NULL,

    CONSTRAINT PK_crmLPTransactionBalances
        PRIMARY KEY (LPTransactionBalanceID),

    CONSTRAINT UQ_crmLPTransactionBalances_CustomerId
        UNIQUE (CustomerID),

    CONSTRAINT FK_crmLPTransactionBalances_Customer
        FOREIGN KEY (CustomerID)
        REFERENCES Customer(CustomerId)
);
GO
