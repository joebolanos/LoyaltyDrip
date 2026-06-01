using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Drip;

public static class GetBalancesEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/drip/balances/{customerId}", HandleAsync);
    }

    private static async Task<Results<Ok<CustomerBalancesResult>, NotFound, ProblemHttpResult>> HandleAsync(
        string customerId,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerBalancesQuery(customerId), ct);

        return result.Match<Results<Ok<CustomerBalancesResult>, NotFound, ProblemHttpResult>>(
            balances => TypedResults.Ok(balances),
            error =>
            {
                if (error.Code == DomainError.NotFound) return TypedResults.NotFound();
                return TypedResults.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest);
            });
    }
}
