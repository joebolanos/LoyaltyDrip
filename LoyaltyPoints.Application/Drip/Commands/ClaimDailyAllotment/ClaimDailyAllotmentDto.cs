namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public class ClaimDailyAllotmentDto
{
    public int AllotmentCredited { get; init; }

    public static ClaimDailyAllotmentDto Map(int allotment) => new()
    {
        AllotmentCredited = allotment,
    };
}
