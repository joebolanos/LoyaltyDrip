namespace LoyaltyPoints.Application.Common;

internal static class SqlQueries
{
    internal static readonly string GetLoyaltyPointsBalanceByUser = "dbo.SP_GetLoyaltyPointsBalanceByUser";

    internal static readonly string GetDripPoolByCustomerId =
        @"SELECT DripPoolId, CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt
          FROM dbo.DripPool
          WHERE CustomerId = @CustomerId";

    internal static readonly string GetAllDripPoolsWithBalance =
        @"SELECT DripPoolId, CustomerId, Balance, CurrentBase, LastRefillAt, UpdatedAt, CreatedAt
          FROM dbo.DripPool
          WHERE Balance > 0";

    internal static readonly string DeductDripPoolBalance =
        @"UPDATE dbo.DripPool
          SET Balance   = Balance - @Amount,
              UpdatedAt = GETUTCDATE()
          WHERE CustomerId = @CustomerId";

    internal static readonly string GetUnclaimedPoolByCustomerId =
        @"SELECT UnclaimedPoolId, CustomerId, Balance, UpdatedAt, CreatedAt
          FROM dbo.UnclaimedPool
          WHERE CustomerId = @CustomerId";

    internal static readonly string AddToUnclaimedBalance =
        @"MERGE dbo.UnclaimedPool AS target
          USING (SELECT @CustomerId AS CustomerId) AS source
              ON target.CustomerId = source.CustomerId
          WHEN MATCHED THEN
              UPDATE SET Balance   = Balance + @Amount,
                         UpdatedAt = GETUTCDATE()
          WHEN NOT MATCHED THEN
              INSERT (CustomerId, Balance, UpdatedAt, CreatedAt)
              VALUES (@CustomerId, @Amount, GETUTCDATE(), GETUTCDATE());";

    internal static readonly string GetSnapshotByCustomerAndDate =
        @"SELECT SnapshotId, CustomerId, CycleDate, Status, ClaimedAt, CreatedAt
          FROM dbo.DailyClaimSnapshot
          WHERE CustomerId = @CustomerId
            AND CycleDate  = @CycleDate";

    internal static readonly string GetAllPendingSnapshotsBefore =
        @"SELECT SnapshotId, CustomerId, CycleDate, Status, ClaimedAt, CreatedAt
          FROM dbo.DailyClaimSnapshot
          WHERE Status    = @Status
            AND CycleDate < @CycleDate";

    internal static readonly string InsertSnapshot =
        @"INSERT INTO dbo.DailyClaimSnapshot (CustomerId, CycleDate, Status, ClaimedAt, CreatedAt)
          VALUES (@CustomerId, @CycleDate, @Status, @ClaimedAt, @CreatedAt)";

    internal static readonly string UpdateSnapshotStatus =
        @"UPDATE dbo.DailyClaimSnapshot
          SET Status    = @Status,
              ClaimedAt = @ClaimedAt
          WHERE SnapshotId = @SnapshotId";

    internal static readonly string GetLPBalanceByCustomerId =
        @"SELECT LPTransactionBalanceID, CustomerID, Balance, LifetimePoints, SeasonPoints,
                 LPTierID, LastTierUpdate, LastLifetimePointsUpdate, LastSeasonPoints, LastLPTierID, Comments
          FROM dbo.crmLPTransactionBalances
          WHERE CustomerID = @CustomerID";

    internal static readonly string AddLPPoints =
        @"UPDATE dbo.crmLPTransactionBalances
          SET Balance                  = Balance + @Amount,
              LifetimePoints           = LifetimePoints + @Amount,
              SeasonPoints             = SeasonPoints + @Amount,
              LastLifetimePointsUpdate = GETUTCDATE()
          WHERE CustomerID = @CustomerID";
}
