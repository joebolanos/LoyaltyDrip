-- =============================================================================
-- Loyalty Drip Mechanic — SP: sp_ClaimDailyAllotment
-- Executes the daily claim atomically using the frozen AllotmentAmount.
-- Never recalculates against DripConfig at claim time.
-- All date logic uses GETUTCDATE() — no timezone mismatch.
--
-- Input parameters:
--   @CustomerId   VARCHAR(10)
--
-- Output parameters:
--   @AllotmentOut INT — LP credited (0 if not processed)
--   @ErrorCode    INT — 0=OK | 1=NOT_FOUND | 2=ALREADY_CLAIMED
--                       3=INSUFFICIENT_BALANCE | 4=ZERO_ALLOTMENT | 5=NO_SNAPSHOT
-- =============================================================================
GO
CREATE  PROCEDURE sp_ClaimDailyAllotment
    @CustomerId     VARCHAR(10),
    @AllotmentOut   INT         OUTPUT,
    @ErrorCode      INT         OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TodayUtc       DATE            = CAST(GETUTCDATE() AS DATE);
    DECLARE @NowUtc         DATETIME2(3)    = SYSUTCDATETIME();
    DECLARE @Allotment      INT             = 0;
    DECLARE @SnapshotId     BIGINT          = NULL;
    DECLARE @SnapshotStatus TINYINT         = NULL;
    DECLARE @CycleDate      DATE            = NULL;

    SET @AllotmentOut = 0;
    SET @ErrorCode    = 0;

    -- Verify customer exists
    IF NOT EXISTS (SELECT 1 FROM crmLPTransactionBalances WHERE CustomerID = @CustomerId)
    BEGIN
        SET @ErrorCode = 1; -- NOT_FOUND
        RETURN;
    END

    -- Load snapshot
    SELECT
        @SnapshotId     = SnapshotId,
        @Allotment      = AllotmentAmount,
        @SnapshotStatus = Status,
        @CycleDate      = CycleDate
    FROM DailyClaimSnapshot
    WHERE CustomerId = @CustomerId;

    -- No snapshot found — player is not eligible today
    IF @SnapshotId IS NULL
    BEGIN
        SET @ErrorCode = 5; -- NO_SNAPSHOT
        RETURN;
    END

    -- Player already claimed today
    IF @SnapshotStatus = 1
    BEGIN
        SET @ErrorCode = 2; -- ALREADY_CLAIMED
        RETURN;
    END

    -- Snapshot belongs to a previous cycle (not reset yet by nightly job)
    IF @CycleDate <> @TodayUtc
    BEGIN
        SET @ErrorCode = 5; -- NO_SNAPSHOT
        RETURN;
    END

    -- Frozen allotment is zero
    IF @Allotment = 0
    BEGIN
        SET @ErrorCode = 4; -- ZERO_ALLOTMENT
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Deduct frozen AllotmentAmount from DripPool
        UPDATE DripPool
        SET    Balance   = Balance - @Allotment,
               UpdatedAt = @NowUtc
        WHERE  CustomerId = @CustomerId
          AND  Balance   >= @Allotment;

        -- Guard against race condition
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SET @ErrorCode = 3; -- INSUFFICIENT_BALANCE
            RETURN;
        END

        -- Credit frozen AllotmentAmount to LP balance
        UPDATE crmLPTransactionBalances
        SET    Balance                  = Balance + @Allotment,
               LifetimePoints           = LifetimePoints + @Allotment,
               SeasonPoints             = SeasonPoints + @Allotment,
               LastLifetimePointsUpdate = @NowUtc
        WHERE  CustomerID = @CustomerId;

        -- Mark snapshot as CLAIMED
        UPDATE DailyClaimSnapshot
        SET    Status    = 1,
               ClaimedAt = @NowUtc,
               UpdatedAt = @NowUtc
        WHERE  SnapshotId = @SnapshotId;

        COMMIT TRANSACTION;

        SET @AllotmentOut = @Allotment;
        SET @ErrorCode    = 0; -- OK

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMsg   NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorLine  INT            = ERROR_LINE();
        RAISERROR('sp_ClaimDailyAllotment failed at line %d: %s', 16, 1, @ErrorLine, @ErrorMsg);
    END CATCH
END
GO
