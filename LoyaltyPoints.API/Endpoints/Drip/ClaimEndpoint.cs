using LoyaltyPoints.API.Extensions;
using LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Drip;

public static class ClaimEndpoint
{
    public static RouteGroupBuilder MapClaimEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/claim", HandleAsync)
            .WithTags("Drip")
            .WithSummary("Claim daily drip allotment")
            .WithName("ClaimDailyAllotment")
            .ProducesProblem(400)
            .ProducesProblem(404)
            .MapToApiVersion(1);

        return group;
    }

    private static async Task<Results<Ok<ClaimDailyAllotmentDto>, ProblemHttpResult>> HandleAsync(
        [FromBody] ClaimRequest request,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ClaimDailyAllotmentCommand(request.CustomerId), ct);

        return result.Match<Results<Ok<ClaimDailyAllotmentDto>, ProblemHttpResult>>(
            dto       => TypedResults.Ok(dto),
            exception => exception.ToProblemResult());
    }

    private record ClaimRequest(string CustomerId);
}
