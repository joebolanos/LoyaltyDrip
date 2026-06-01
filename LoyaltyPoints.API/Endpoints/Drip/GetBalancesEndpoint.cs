using LoyaltyPoints.API.Extensions;
using LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Drip;

public static class GetBalancesEndpoint
{
    public static RouteGroupBuilder MapGetBalancesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/balances/{customerId}", HandleAsync)
            .WithTags("Drip")
            .WithSummary("Get customer loyalty balances")
            .WithName("GetCustomerBalances")
            .ProducesProblem(404)
            .MapToApiVersion(1);

        return group;
    }

    private static async Task<Results<Ok<CustomerBalancesDto>, ProblemHttpResult>> HandleAsync(
        string customerId,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerBalancesQuery(customerId), ct);

        return result.Match<Results<Ok<CustomerBalancesDto>, ProblemHttpResult>>(
            dto       => TypedResults.Ok(dto),
            exception => exception.ToProblemResult());
    }
}
