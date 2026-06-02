using System.Data;
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

            var result = await sqlConnection.QueryFirstOrDefaultAsync<SpNightlyJobResult>(
                "sp_ProcessNightlyDripJob",
                commandType: CommandType.StoredProcedure);

            return NightlyJobDto.Map(
                wagersProcessed:  result?.WagersProcessed ?? 0,
                forfeitsApplied:  result?.ForfeitsApplied ?? 0,
                snapshotsCreated: result?.SnapshotsCreated ?? 0);
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
