-- =============================================================================
-- Loyalty Drip Mechanic — SP: sp_ProcessNightlyDripJob
-- Reads active configuration from DripConfig.
-- Uses ImmediateCreditRatio for LP credits and DripPoolRatio for DripPool credits
-- as independent configurable values.
-- Freezes AllotmentAmount in the snapshot at job run time.
-- Uses frozen AllotmentAmount for forfeit — never recalculates.
-- All date logic uses GETUTCDATE() — no timezone mismatch.
--
-- Steps (all within a single transaction):
--   1. FORFEIT  — PENDING snapshots from previous cycles → UnclaimedPool
--                 using the frozen AllotmentAmount
--   2. RELOAD   — unprocessed wagers → crmLPTransactionBalances + DripPool
--                 using independent ImmediateCreditRatio and DripPoolRatio
--   3. SNAPSHOT — eligible customers → MERGE into DailyClaimSnapshot
--                 freezes the new AllotmentAmount for the current cycle
--
-- Result (one row):
--   WagersProcessed  INT
--   ForfeitsApplied  INT
--   SnapshotsCreated INT
-- =============================================================================
GO
CREATE PROCEDURE sp_ProcessNightlyDripJob
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today                  DATE            = CAST(GETUTCDATE() AS DATE);
    DECLARE @NowUtc                 DATETIME2(3)    = SYSUTCDATETIME();
    DECLARE @ImmediateCreditRatio   DECIMAL(5,2)    = 1.00;
    DECLARE @DripPoolRatio          DECIMAL(5,2)    = 1.00;
    DECLARE @Percentage             DECIMAL(5,2)    = 0.20;
    DECLARE @MinBalance             INT             = 0;

    -- Load active configuration
    SELECT TOP 1
        @ImmediateCreditRatio   = ImmediateCreditRatio,
        @DripPoolRatio          = DripPoolRatio,
        @Percentage             = DailyAllotmentPercent,
        @MinBalance             = MinimumBalanceForClaim
    FROM DripConfig
    WHERE IsActive = 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- =====================================================================
        -- STEP 1 — FORFEIT
        -- PENDING snapshots from previous cycles → move to UnclaimedPool
        -- Uses the frozen AllotmentAmount — does not recalculate
        -- =====================================================================
        DECLARE @SnapshotId     BIGINT;
        DECLARE @CustomerId     VARCHAR(10);
        DECLARE @FrozenAmount   INT;

        DECLARE forfeit_cursor CURSOR FOR
            SELECT SnapshotId, CustomerId, AllotmentAmount
            FROM   DailyClaimSnapshot
            WHERE  Status          = 0      -- PENDING
              AND  AllotmentAmount > 0;

        OPEN forfeit_cursor;
        FETCH NEXT FROM forfeit_cursor INTO @SnapshotId, @CustomerId, @FrozenAmount;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Deduct frozen amount from DripPool
            UPDATE DripPool
            SET    Balance   = Balance - @FrozenAmount,
                   UpdatedAt = @NowUtc
            WHERE  CustomerId = @CustomerId
              AND  Balance   >= @FrozenAmount;

            -- Move to UnclaimedPool only if deduction succeeded
            IF @@ROWCOUNT > 0
            BEGIN
                MERGE UnclaimedPool AS target
                USING (SELECT @CustomerId AS CustomerId) AS source
                    ON target.CustomerId = source.CustomerId
                WHEN MATCHED THEN
                    UPDATE SET Balance   = Balance + @FrozenAmount,
                               UpdatedAt = @NowUtc
                WHEN NOT MATCHED THEN
                    INSERT (CustomerId, Balance, UpdatedAt, CreatedAt)
                    VALUES (@CustomerId, @FrozenAmount, @NowUtc, @NowUtc);
            END

            -- Mark snapshot as FORFEITED
            UPDATE DailyClaimSnapshot
            SET    Status    = 2,
                   UpdatedAt = @NowUtc
            WHERE  SnapshotId = @SnapshotId;

            FETCH NEXT FROM forfeit_cursor INTO @SnapshotId, @CustomerId, @FrozenAmount;
        END

        CLOSE forfeit_cursor;
        DEALLOCATE forfeit_cursor;

        -- =====================================================================
        -- STEP 2 — RELOAD
        -- Unprocessed wagers → credit crmLPTransactionBalances + DripPool
        -- ImmediateCreditRatio and DripPoolRatio are independent values
        -- =====================================================================
        DECLARE @TransactionId      BIGINT;
        DECLARE @WagerAmount        INT;
        DECLARE @ImmediateLPToAdd   INT;
        DECLARE @DripLPToAdd        INT;

        DECLARE wager_cursor CURSOR FOR
            SELECT TransactionId, CustomerId, WagerAmount
            FROM   crmLPTransactions
            WHERE  Processed = 0;

        OPEN wager_cursor;
        FETCH NEXT FROM wager_cursor INTO @TransactionId, @CustomerId, @WagerAmount;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @ImmediateLPToAdd = FLOOR(@WagerAmount * @ImmediateCreditRatio);
            SET @DripLPToAdd      = FLOOR(@WagerAmount * @DripPoolRatio);

            -- Credit immediate LP to spendable balance
            UPDATE crmLPTransactionBalances
            SET    Balance                  = Balance + @ImmediateLPToAdd,
                   LifetimePoints           = LifetimePoints + @ImmediateLPToAdd,
                   SeasonPoints             = SeasonPoints + @ImmediateLPToAdd,
                   LastLifetimePointsUpdate = @NowUtc
            WHERE  CustomerID = @CustomerId;

            -- Credit DripPool separately (create if first wager for this customer)
            MERGE DripPool AS target
            USING (SELECT @CustomerId AS CustomerId) AS source
                ON target.CustomerId = source.CustomerId
            WHEN MATCHED THEN
                UPDATE SET Balance      = Balance + @DripLPToAdd,
                           LastRefillAt = @NowUtc,
                           UpdatedAt    = @NowUtc
            WHEN NOT MATCHED THEN
                INSERT (CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt)
                VALUES (@CustomerId, @DripLPToAdd, 0, @NowUtc, @NowUtc, @NowUtc);

            -- Mark wager as processed
            UPDATE crmLPTransactions
            SET    Processed   = 1,
                   ProcessedAt = @NowUtc
            WHERE  TransactionId = @TransactionId;

            FETCH NEXT FROM wager_cursor INTO @TransactionId, @CustomerId, @WagerAmount;
        END

        CLOSE wager_cursor;
        DEALLOCATE wager_cursor;

        -- =====================================================================
        -- STEP 3 — SNAPSHOT
        -- Eligible customers → MERGE into DailyClaimSnapshot
        -- Freezes AllotmentAmount calculated with current config
        -- Only updates if customer meets MinimumBalanceForClaim
        -- =====================================================================
        MERGE DailyClaimSnapshot AS target
        USING (
            SELECT
                dp.CustomerId,
                @Today                          AS CycleDate,
                FLOOR(dp.Balance * @Percentage) AS AllotmentAmount,
                0                               AS Status,
                @NowUtc                         AS NowUtc
            FROM DripPool dp
            WHERE dp.Balance >= @MinBalance
              AND FLOOR(dp.Balance * @Percentage) > 0
        ) AS source ON target.CustomerId = source.CustomerId
        WHEN MATCHED THEN
            -- Reset existing record with new cycle date and frozen allotment
            UPDATE SET
                CycleDate       = source.CycleDate,
                AllotmentAmount = source.AllotmentAmount,
                Status          = 0,        -- PENDING
                ClaimedAt       = NULL,
                UpdatedAt       = source.NowUtc
        WHEN NOT MATCHED THEN
            -- First snapshot for this customer
            INSERT (CustomerId, CycleDate, AllotmentAmount, Status, ClaimedAt, CreatedAt, UpdatedAt)
            VALUES (source.CustomerId, source.CycleDate, source.AllotmentAmount, 0, NULL, source.NowUtc, source.NowUtc);

        COMMIT TRANSACTION;

        -- Job result summary
        SELECT
            (SELECT COUNT(*) FROM crmLPTransactions
             WHERE  Processed = 1
               AND  ProcessedAt >= DATEADD(MINUTE, -1, @NowUtc))    AS WagersProcessed,
            (SELECT COUNT(*) FROM DailyClaimSnapshot
             WHERE  Status = 2
               AND  CycleDate < @Today)                              AS ForfeitsApplied,
            (SELECT COUNT(*) FROM DailyClaimSnapshot
             WHERE  Status = 0
               AND  CycleDate = @Today)                              AS SnapshotsCreated;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMsg   NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorLine  INT            = ERROR_LINE();
        RAISERROR('sp_ProcessNightlyDripJob failed at line %d: %s', 16, 1, @ErrorLine, @ErrorMsg);
    END CATCH
END
GO
