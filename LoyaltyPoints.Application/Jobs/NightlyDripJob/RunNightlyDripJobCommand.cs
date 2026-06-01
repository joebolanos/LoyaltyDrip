using LoyaltyPoints.Application.Common;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public record RunNightlyDripJobCommand(string CustomerId)
    : IRequest<OneOf<NightlyJobResult, DomainError>>;

public record NightlyJobResult(int CustomersProcessed, int ForfeitsApplied);
