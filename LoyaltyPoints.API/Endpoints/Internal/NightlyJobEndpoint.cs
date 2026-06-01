using LoyaltyPoints.API.Extensions;
using LoyaltyPoints.Application.Jobs.NightlyDripJob;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Internal;

public static class NightlyJobEndpoint
{
    public static RouteGroupBuilder MapNightlyJobEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/nightly-job", HandleAsync)
            .WithTags("Internal")
            .WithSummary("Run nightly drip forfeit job")
            .WithName("RunNightlyDripJob")
            .ProducesProblem(400)
            .MapToApiVersion(1);

        return group;
    }

    private static async Task<Results<Ok<NightlyJobDto>, ProblemHttpResult>> HandleAsync(
        [FromBody] NightlyJobRequest request,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RunNightlyDripJobCommand(request.CustomerId), ct);

        return result.Match<Results<Ok<NightlyJobDto>, ProblemHttpResult>>(
            dto       => TypedResults.Ok(dto),
            exception => exception.ToProblemResult());
    }

    private record NightlyJobRequest(string CustomerId);
}
