namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public class NightlyJobDto
{
    public int WagersProcessed { get; set; }
    public int ForfeitsApplied { get; set; }
    public int SnapshotsCreated { get; set; }

    public static NightlyJobDto Map(int wagersProcessed, int forfeitsApplied, int snapshotsCreated) => new()
    {
        WagersProcessed  = wagersProcessed,
        ForfeitsApplied  = forfeitsApplied,
        SnapshotsCreated = snapshotsCreated,
    };
}
