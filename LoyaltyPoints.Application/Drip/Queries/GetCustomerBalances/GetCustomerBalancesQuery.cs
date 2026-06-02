using System.Data;
using Core.Shared.Common;
using Core.Shared.Exceptions;
using Dapper;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Application.Common;
using MediatR;

namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public sealed record GetCustomerBalancesQuery(string CustomerId) : IRequest<Result<CustomerBalancesDto>>;

internal sealed class GetCustomerBalancesHandler(ISqlConnectionFactory connection)
    : IRequestHandler<GetCustomerBalancesQuery, Result<CustomerBalancesDto>>
{
    public async Task<Result<CustomerBalancesDto>> Handle(
        GetCustomerBalancesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var sqlConnection = connection.CreateConnection();
            await sqlConnection.OpenAsync(cancellationToken);

            var dto = await sqlConnection.QuerySingleOrDefaultAsync<CustomerBalancesDto>(
                SqlQueries.GetLoyaltyPointsBalanceByUser,
                new { request.CustomerId },
                commandType: CommandType.StoredProcedure);

            if (dto is null)
                return new NotFoundException("Customer not found.");

            return dto;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
