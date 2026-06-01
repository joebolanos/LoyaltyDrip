namespace LoyaltyPoints.Application.Common;

public record DomainError(string Code, string Message)
{
    public const string NotFound          = "NOT_FOUND";
    public const string AlreadyClaimed    = "ALREADY_CLAIMED";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
}
