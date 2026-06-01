using Core.Shared.Common;
using Core.Shared.Exceptions;
using LoyaltyPoints.Application.Abstractions;
using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain.Entities;
using MediatR;
using Dapper;



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

            var lpBalance = await sqlConnection.QuerySingleOrDefaultAsync<CrmLPTransactionBalances>(
                SqlQueries.GetLPBalanceByCustomerId, new { CustomerID = request.CustomerId });

            if (lpBalance is null)
                return new NotFoundException("Customer not found.");

            var dripPool = await sqlConnection.QuerySingleOrDefaultAsync<DripPool>(
                SqlQueries.GetDripPoolByCustomerId, new { request.CustomerId });

            var unclaimedPool = await sqlConnection.QuerySingleOrDefaultAsync<UnclaimedPool>(
                SqlQueries.GetUnclaimedPoolByCustomerId, new { request.CustomerId });

            return CustomerBalancesDto.Map(lpBalance, dripPool, unclaimedPool);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}