-- =============================================================================
-- Loyalty Drip Mechanic — TABLE: Customer
-- Root entity. All loyalty balances hang from this table.
-- For development and testing purposes only.
-- =============================================================================
CREATE TABLE Customer (
    CustomerId  VARCHAR(10)     NOT NULL,
    FullName    VARCHAR(100)    NOT NULL,
    Email       VARCHAR(150)    NOT NULL,
    IsActive    BIT             NOT NULL    DEFAULT 1,
    CreatedAt   DATETIME2(3)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Customer
        PRIMARY KEY (CustomerId),

    CONSTRAINT UQ_Customer_Email
        UNIQUE (Email)
);
GO
