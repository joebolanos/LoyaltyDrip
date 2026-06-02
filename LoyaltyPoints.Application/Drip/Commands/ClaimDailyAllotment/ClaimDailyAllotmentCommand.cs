using System.Data;
using Core.Shared.Common;
using Core.Shared.Exceptions;
using Dapper;
using FluentValidation;
using LoyaltyPoints.Application.Abstractions;
using MediatR;

namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public sealed record ClaimDailyAllotmentCommand(string CustomerId)
    : IRequest<Result<ClaimDailyAllotmentDto>>;

internal sealed class ClaimDailyAllotmentHandler(
    ISqlConnectionFactory connection,
    IValidator<ClaimDailyAllotmentCommand> validator)
    : IRequestHandler<ClaimDailyAllotmentCommand, Result<ClaimDailyAllotmentDto>>
{
    public async Task<Result<ClaimDailyAllotmentDto>> Handle(
        ClaimDailyAllotmentCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationException(validationResult.Errors);

        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("CustomerId", request.CustomerId);
            parameters.Add("AllotmentOut", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("ErrorCode",   dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            await sqlConnection.ExecuteAsync(
                "sp_ClaimDailyAllotment", parameters,
                commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int>("ErrorCode");

            return errorCode switch
            {
                0 => ClaimDailyAllotmentDto.Map(parameters.Get<int>("AllotmentOut")),
                1 => new NotFoundException("No active drip pool found for this customer."),
                2 => new InvalidOperationException("Ya reclamaste tu reward de hoy."),
                3 => new InvalidOperationException("Insufficient drip pool balance."),
                4 => new InvalidOperationException("Drip pool balance is too low to generate an allotment."),
                _ => new InvalidOperationException($"Unexpected error from sp_ClaimDailyAllotment: {errorCode}.")
            };
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
