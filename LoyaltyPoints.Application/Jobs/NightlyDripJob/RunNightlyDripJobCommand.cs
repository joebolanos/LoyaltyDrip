using Core.Shared.Common;
using Dapper;
using LoyaltyPoints.Application.Abstractions;
using MediatR;

namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public sealed record RunNightlyDripJobCommand : IRequest<Result<NightlyJobDto>>;

internal sealed class RunNightlyDripJobHandler(ISqlConnectionFactory connection) : IRequestHandler<RunNightlyDripJobCommand, Result<NightlyJobDto>>
{
    public async Task<Result<NightlyJobDto>> Handle(RunNightlyDripJobCommand request, CancellationToken cancellationToken)
    {
        try
        {
            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QuerySingleAsync<SpNightlyJobResult>(
                "EXEC sp_ProcessNightlyDripJob");

            return NightlyJobDto.Map(
                customersProcessed: result.WagersProcessed,
                forfeitsApplied:    result.ForfeitsApplied,
                snapshotsCreated:   result.SnapshotsCreated);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private sealed record SpNightlyJobResult(
        int WagersProcessed,
        int ForfeitsApplied,
        int SnapshotsCreated);
}
