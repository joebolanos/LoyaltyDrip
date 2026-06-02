namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public class NightlyJobDto
{
    public int CustomersProcessed { get; set; }
    public int ForfeitsApplied { get; set; }
    public int SnapshotsCreated { get; set; }

    public static NightlyJobDto Map(int customersProcessed, int forfeitsApplied, int snapshotsCreated) => new()
    {
        CustomersProcessed = customersProcessed,
        ForfeitsApplied    = forfeitsApplied,
        SnapshotsCreated   = snapshotsCreated,
    };
}
