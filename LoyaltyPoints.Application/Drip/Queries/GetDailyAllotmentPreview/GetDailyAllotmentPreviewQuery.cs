using System.Data;
using Core.Shared.Common;
using Core.Shared.Exceptions;
using Dapper;
using LoyaltyPoints.Application.Abstractions;
using MediatR;

namespace LoyaltyPoints.Application.Drip.Queries.GetDailyAllotmentPreview;

public sealed record GetDailyAllotmentPreviewQuery(string CustomerId)
    : IRequest<Result<AllotmentPreviewDto>>;

internal sealed class GetDailyAllotmentPreviewHandler(ISqlConnectionFactory connection)
    : IRequestHandler<GetDailyAllotmentPreviewQuery, Result<AllotmentPreviewDto>>
{
    public async Task<Result<AllotmentPreviewDto>> Handle(
        GetDailyAllotmentPreviewQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            var result = await sqlConnection.QuerySingleOrDefaultAsync<AllotmentPreviewDto>(
                "sp_GetDailyAllotmentPreview",
                new { CustomerId = request.CustomerId },
                commandType: CommandType.StoredProcedure);

            if (result is null)
                return new NotFoundException("Customer not found.");

            return result;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
