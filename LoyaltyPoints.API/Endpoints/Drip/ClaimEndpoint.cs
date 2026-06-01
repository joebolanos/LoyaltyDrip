using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyPoints.API.Endpoints.Drip;

public static class ClaimEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/drip/claim", HandleAsync);
    }

    private static async Task<Results<Ok<ClaimDailyAllotmentResult>, NotFound, ProblemHttpResult>> HandleAsync(
        [FromBody] ClaimRequest request,
        [FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ClaimDailyAllotmentCommand(request.CustomerId), ct);

        return result.Match<Results<Ok<ClaimDailyAllotmentResult>, NotFound, ProblemHttpResult>>(
            success => TypedResults.Ok(success),
            error =>
            {
                if (error.Code == DomainError.NotFound) return TypedResults.NotFound();
                return TypedResults.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest);
            });
    }

    private record ClaimRequest(string CustomerId);
}
