namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public class NightlyJobDto
{
    public int CustomersProcessed { get; set; }
    public int ForfeitsApplied { get; set; }

    public static NightlyJobDto Map(int customersProcessed, int forfeitsApplied) => new()
    {
        CustomersProcessed = customersProcessed,
        ForfeitsApplied    = forfeitsApplied,
    };
}
