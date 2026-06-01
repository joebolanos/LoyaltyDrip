using Core.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LoyaltyPoints.API.Extensions;

public static class ExceptionExtensions
{
    public static ProblemHttpResult ToProblemResult(this Exception exception) =>
        exception switch
        {
            NotFoundException nfe =>
                TypedResults.Problem(nfe.Message, statusCode: StatusCodes.Status404NotFound),
            ValidationException vex =>
                TypedResults.Problem(
                    string.Join("; ", vex.Errors.Select(e => e.ErrorMessage)),
                    statusCode: StatusCodes.Status400BadRequest),
            ForbiddenException ffe =>
                TypedResults.Problem(ffe.Message, statusCode: StatusCodes.Status403Forbidden),
            _ =>
                TypedResults.Problem(exception.Message, statusCode: StatusCodes.Status400BadRequest),
        };
}
