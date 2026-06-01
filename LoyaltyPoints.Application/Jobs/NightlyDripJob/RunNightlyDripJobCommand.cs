using Core.Shared.Common;
using FluentValidation;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain.Entities;
using MediatR;
using Dapper;

namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public sealed record RunNightlyDripJobCommand(string CustomerId) : IRequest<Result<NightlyJobDto>>;

internal sealed class RunNightlyDripJobHandler(ISqlConnectionFactory connection, IValidator<RunNightlyDripJobCommand> validator)
    : IRequestHandler<RunNightlyDripJobCommand, Result<NightlyJobDto>>
{
    private const decimal AllotmentPercentage = 0.20m;

    public async Task<Result<NightlyJobDto>> Handle(
        RunNightlyDripJobCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new ValidationException(validationResult.Errors);

        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            var pendingSnapshots = (await sqlConnection.QueryAsync<DailyClaimSnapshot>(
                SqlQueries.GetAllPendingSnapshotsBefore,
                new { Status = (byte)ClaimStatus.Pending, CycleDate = today.ToDateTime(TimeOnly.MinValue) }))
                .Where(s => s.CustomerId == request.CustomerId)
                .ToList();

            int forfeitsApplied = 0;

            using var tx = sqlConnection.BeginTransaction();
            try
            {
                foreach (var snapshot in pendingSnapshots)
                {
                    var dripPool = await sqlConnection.QuerySingleOrDefaultAsync<DripPool>(
                        SqlQueries.GetDripPoolByCustomerId,
                        new { snapshot.CustomerId },
                        tx);

                    if (dripPool is { Balance: > 0 })
                    {
                        int allotment = (int)Math.Floor(dripPool.Balance * AllotmentPercentage);

                        if (allotment > 0)
                        {
                            await sqlConnection.ExecuteAsync(
                                SqlQueries.AddToUnclaimedBalance,
                                new { snapshot.CustomerId, Amount = allotment },
                                tx);

                            await sqlConnection.ExecuteAsync(
                                SqlQueries.DeductDripPoolBalance,
                                new { snapshot.CustomerId, Amount = allotment },
                                tx);
                        }
                    }

                    await sqlConnection.ExecuteAsync(
                        SqlQueries.UpdateSnapshotStatus,
                        new { snapshot.SnapshotId, Status = (byte)ClaimStatus.Forfeited, ClaimedAt = (DateTime?)null },
                        tx);

                    forfeitsApplied++;
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return NightlyJobDto.Map(customersProcessed: 1, forfeitsApplied: forfeitsApplied);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
