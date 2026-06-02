using LoyaltyPoints.API.Extensions;
using LoyaltyPoints.Application.Operator.Commands.SetDripConfig;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Internal;

public static class DripConfigEndpoint
{
    public static RouteGroupBuilder MapDripConfigEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/config", HandleAsync)
            .WithTags("Internal")
            .WithSummary("Insert new active drip configuration")
            .WithName("SetDripConfig")
            .ProducesProblem(400)
            .MapToApiVersion(1);

        return group;
    }

    private static async Task<Results<Ok<DripConfigDto>, ProblemHttpResult>> HandleAsync(
        [FromBody] SetDripConfigRequest request,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new SetDripConfigCommand(
                request.ImmediateCreditRatio,
                request.DripPoolRatio,
                request.DailyAllotmentPercent,
                request.MinimumBalanceForClaim,
                request.ChangedBy),
            ct);

        return result.Match<Results<Ok<DripConfigDto>, ProblemHttpResult>>(
            dto       => TypedResults.Ok(dto),
            exception => exception.ToProblemResult());
    }

    private record SetDripConfigRequest(
        decimal ImmediateCreditRatio,
        decimal DripPoolRatio,
        decimal DailyAllotmentPercent,
        int MinimumBalanceForClaim,
        string ChangedBy);
}
