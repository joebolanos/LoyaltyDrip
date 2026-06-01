using LoyaltyPoints.Application.Jobs.NightlyDripJob;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Internal;

public static class NightlyJobEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/drip/nightly-job", HandleAsync);
    }

    private static async Task<Results<Ok<NightlyJobResult>, ProblemHttpResult>> HandleAsync(
        [FromBody] NightlyJobRequest request,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RunNightlyDripJobCommand(request.CustomerId), ct);

        return result.Match<Results<Ok<NightlyJobResult>, ProblemHttpResult>>(
            jobResult => TypedResults.Ok(jobResult),
            error => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
    }

    private record NightlyJobRequest(string CustomerId);
}
