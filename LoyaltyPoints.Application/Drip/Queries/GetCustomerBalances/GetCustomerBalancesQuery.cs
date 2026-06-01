using LoyaltyPoints.Application.Common;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public record GetCustomerBalancesQuery(string CustomerId)
    : IRequest<OneOf<CustomerBalancesResult, DomainError>>;

public record CustomerBalancesResult(int LpBalance, int DripPoolBalance, int UnclaimedBalance);
