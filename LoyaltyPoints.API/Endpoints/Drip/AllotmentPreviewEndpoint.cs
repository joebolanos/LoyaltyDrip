using LoyaltyPoints.API.Extensions;
using LoyaltyPoints.Application.Drip.Queries.GetDailyAllotmentPreview;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Drip;

public static class AllotmentPreviewEndpoint
{
    public static RouteGroupBuilder MapAllotmentPreviewEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/claim-preview/{customerId}", HandleAsync)
            .WithTags("Drip")
            .WithSummary("Preview daily allotment before claiming")
            .WithName("GetDailyAllotmentPreview")
            .ProducesProblem(404)
            .MapToApiVersion(1);

        return group;
    }

    private static async Task<Results<Ok<AllotmentPreviewDto>, ProblemHttpResult>> HandleAsync(
        string customerId,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetDailyAllotmentPreviewQuery(customerId), ct);

        return result.Match<Results<Ok<AllotmentPreviewDto>, ProblemHttpResult>>(
            dto       => TypedResults.Ok(dto),
            exception => exception.ToProblemResult());
    }
}
