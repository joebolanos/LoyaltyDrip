using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain.Repositories;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Drip.Queries.GetCustomerBalances;

public sealed class GetCustomerBalancesHandler
    : IRequestHandler<GetCustomerBalancesQuery, OneOf<CustomerBalancesResult, DomainError>>
{
    private readonly ILPBalanceRepository _lpBalanceRepo;
    private readonly IDripPoolRepository _dripPoolRepo;
    private readonly IUnclaimedPoolRepository _unclaimedPoolRepo;

    public GetCustomerBalancesHandler(
        ILPBalanceRepository lpBalanceRepo,
        IDripPoolRepository dripPoolRepo,
        IUnclaimedPoolRepository unclaimedPoolRepo)
    {
        _lpBalanceRepo     = lpBalanceRepo;
        _dripPoolRepo      = dripPoolRepo;
        _unclaimedPoolRepo = unclaimedPoolRepo;
    }

    public async Task<OneOf<CustomerBalancesResult, DomainError>> Handle(
        GetCustomerBalancesQuery request, CancellationToken cancellationToken)
    {
        var lpTask        = _lpBalanceRepo.GetByCustomerIdAsync(request.CustomerId);
        var dripTask      = _dripPoolRepo.GetByCustomerIdAsync(request.CustomerId);
        var unclaimedTask = _unclaimedPoolRepo.GetByCustomerIdAsync(request.CustomerId);

        await Task.WhenAll(lpTask, dripTask, unclaimedTask);

        var lpBalance = await lpTask;
        if (lpBalance is null)
            return new DomainError(DomainError.NotFound, "Customer not found.");

        return new CustomerBalancesResult(
            LpBalance:       (int)lpBalance.Balance,
            DripPoolBalance: (await dripTask)?.Balance ?? 0,
            UnclaimedBalance: (await unclaimedTask)?.Balance ?? 0);
    }
}
