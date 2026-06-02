namespace LoyaltyPoints.Application.Operator.Commands.SetDripConfig;

public class DripConfigDto
{
    public int ConfigId { get; init; }

    public static DripConfigDto Map(int configId) => new() { ConfigId = configId };
}
