using LoyaltyPoints.Application.Common;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public record ClaimDailyAllotmentCommand(string CustomerId)
    : IRequest<OneOf<ClaimDailyAllotmentResult, DomainError>>;

public record ClaimDailyAllotmentResult(int AllotmentCredited);
