using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public sealed class ClaimDailyAllotmentHandler
    : IRequestHandler<ClaimDailyAllotmentCommand, OneOf<ClaimDailyAllotmentResult, DomainError>>
{
    private const decimal AllotmentPercentage = 0.20m;

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDripPoolRepository _dripPoolRepo;
    private readonly ILPBalanceRepository _lpBalanceRepo;
    private readonly IDailyClaimSnapshotRepository _snapshotRepo;

    public ClaimDailyAllotmentHandler(
        IDbConnectionFactory connectionFactory,
        IDripPoolRepository dripPoolRepo,
        ILPBalanceRepository lpBalanceRepo,
        IDailyClaimSnapshotRepository snapshotRepo)
    {
        _connectionFactory = connectionFactory;
        _dripPoolRepo      = dripPoolRepo;
        _lpBalanceRepo     = lpBalanceRepo;
        _snapshotRepo      = snapshotRepo;
    }

    public async Task<OneOf<ClaimDailyAllotmentResult, DomainError>> Handle(
        ClaimDailyAllotmentCommand request, CancellationToken cancellationToken)
    {
        // 1. DripPool debe existir con balance positivo
        var dripPool = await _dripPoolRepo.GetByCustomerIdAsync(request.CustomerId);
        if (dripPool is null || dripPool.Balance == 0)
            return new DomainError(DomainError.NotFound, "No active drip pool found for this customer.");

        // 2. Prevenir doble claim
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _snapshotRepo.GetByCustomerAndDateAsync(request.CustomerId, today);
        if (existing is not null)
            return new DomainError(DomainError.AlreadyClaimed, "Ya reclamaste tu reward de hoy.");

        // 3. Calcular allotment: Balance × 20%, redondeado hacia abajo
        int allotment = (int)Math.Floor(dripPool.Balance * AllotmentPercentage);
        if (allotment == 0)
            return new DomainError(DomainError.InsufficientBalance,
                "El balance del drip pool no genera un allotment positivo.");

        // 4. Atómica: restar del pool → acreditar LP → registrar snapshot
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            await _dripPoolRepo.DeductBalanceAsync(request.CustomerId, allotment, tx);
            await _lpBalanceRepo.AddPointsAsync(request.CustomerId, allotment, tx);
            await _snapshotRepo.InsertAsync(new DailyClaimSnapshot(request.CustomerId, today), tx);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        return new ClaimDailyAllotmentResult(allotment);
    }
}
