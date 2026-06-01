using Core.Shared.Common;
using Core.Shared.Exceptions;
using FluentValidation;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain.Entities;
using MediatR; 
using Dapper;


namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public sealed record ClaimDailyAllotmentCommand(string CustomerId)
    : IRequest<Result<ClaimDailyAllotmentDto>>;



internal sealed class ClaimDailyAllotmentHandler(
    ISqlConnectionFactory connection,
    IValidator<ClaimDailyAllotmentCommand> validator)
    : IRequestHandler<ClaimDailyAllotmentCommand, Result<ClaimDailyAllotmentDto>>
{
    private const decimal AllotmentPercentage = 0.20m;

    public async Task<Result<ClaimDailyAllotmentDto>> Handle(
        ClaimDailyAllotmentCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationException(validationResult.Errors);

        try
        {
            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            var dripPool = await sqlConnection.QuerySingleOrDefaultAsync<DripPool>(
                SqlQueries.GetDripPoolByCustomerId, new { request.CustomerId });

            if (dripPool is null || dripPool.Balance == 0)
                return new NotFoundException("No active drip pool found for this customer.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var existing = await sqlConnection.QuerySingleOrDefaultAsync<DailyClaimSnapshot>(
                SqlQueries.GetSnapshotByCustomerAndDate,
                new { request.CustomerId, CycleDate = today.ToDateTime(TimeOnly.MinValue) });

            if (existing is not null)
                return new InvalidOperationException("Ya reclamaste tu reward de hoy.");

            int allotment = (int)Math.Floor(dripPool.Balance * AllotmentPercentage);
            if (allotment == 0)
                return new InvalidOperationException("El balance del drip pool no genera un allotment positivo.");

            using var tx = sqlConnection.BeginTransaction();
            try
            {
                await sqlConnection.ExecuteAsync(
                    SqlQueries.DeductDripPoolBalance,
                    new { request.CustomerId, Amount = allotment },
                    tx);

                await sqlConnection.ExecuteAsync(
                    SqlQueries.AddLPPoints,
                    new { CustomerID = request.CustomerId, Amount = allotment },
                    tx);

                await sqlConnection.ExecuteAsync(
                    SqlQueries.InsertSnapshot,
                    new
                    {
                        request.CustomerId,
                        CycleDate = today.ToDateTime(TimeOnly.MinValue),
                        Status = (byte)ClaimStatus.Claimed,
                        ClaimedAt = (DateTime?)DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                    },
                    tx);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return ClaimDailyAllotmentDto.Map(allotment);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
