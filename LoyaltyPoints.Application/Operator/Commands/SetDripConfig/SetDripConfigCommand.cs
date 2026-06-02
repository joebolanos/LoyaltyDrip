using Core.Shared.Common;
using Dapper;
using FluentValidation;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Application.Common;
using MediatR;

namespace LoyaltyPoints.Application.Operator.Commands.SetDripConfig;

public sealed record SetDripConfigCommand(
    decimal ImmediateCreditRatio,
    decimal DripPoolRatio,
    decimal DailyAllotmentPercent,
    int MinimumBalanceForClaim,
    string ChangedBy) : IRequest<Result<DripConfigDto>>;

internal sealed class SetDripConfigHandler(
    ISqlConnectionFactory connection,
    IValidator<SetDripConfigCommand> validator)
    : IRequestHandler<SetDripConfigCommand, Result<DripConfigDto>>
{
    public async Task<Result<DripConfigDto>> Handle(
        SetDripConfigCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationException(validationResult.Errors);

        try
        {
            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);
            using var tx = sqlConnection.BeginTransaction();

            try
            {
                await sqlConnection.ExecuteAsync(
                    SqlQueries.DeactivateAllDripConfigs,
                    transaction: tx);

                var configId = await sqlConnection.ExecuteScalarAsync<int>(
                    SqlQueries.InsertDripConfig,
                    new
                    {
                        request.ImmediateCreditRatio,
                        request.DripPoolRatio,
                        request.DailyAllotmentPercent,
                        request.MinimumBalanceForClaim,
                        request.ChangedBy,
                    },
                    transaction: tx);

                tx.Commit();
                return DripConfigDto.Map(configId);
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
