namespace LoyaltyPoints.Application.Common;

internal static class SqlQueries
{
    internal static readonly string GetLoyaltyPointsBalanceByUser = "dbo.SP_GetLoyaltyPointsBalanceByUser";

    internal static readonly string DeactivateAllDripConfigs =
        "UPDATE DripConfig SET IsActive = 0 WHERE IsActive = 1";

    internal static readonly string InsertDripConfig =
        @"INSERT INTO DripConfig
              (ImmediateCreditRatio, DripPoolRatio, DailyAllotmentPercent, MinimumBalanceForClaim, IsActive, ChangedBy, CreatedAt)
          OUTPUT INSERTED.ConfigId
          VALUES
              (@ImmediateCreditRatio, @DripPoolRatio, @DailyAllotmentPercent, @MinimumBalanceForClaim, 1, @ChangedBy, GETUTCDATE())";
}
