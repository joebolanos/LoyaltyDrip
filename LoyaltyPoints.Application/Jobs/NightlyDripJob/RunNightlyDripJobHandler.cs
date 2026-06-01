using LoyaltyPoints.Application.Common;
using LoyaltyPoints.Domain;
using LoyaltyPoints.Domain.Entities;
using LoyaltyPoints.Domain.Repositories;
using MediatR;
using OneOf;

namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public sealed class RunNightlyDripJobHandler
    : IRequestHandler<RunNightlyDripJobCommand, OneOf<NightlyJobResult, DomainError>>
{
    private const decimal AllotmentPercentage = 0.20m;

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDailyClaimSnapshotRepository _snapshotRepo;
    private readonly IDripPoolRepository _dripPoolRepo;
    private readonly IUnclaimedPoolRepository _unclaimedPoolRepo;

    public RunNightlyDripJobHandler(
        IDbConnectionFactory connectionFactory,
        IDailyClaimSnapshotRepository snapshotRepo,
        IDripPoolRepository dripPoolRepo,
        IUnclaimedPoolRepository unclaimedPoolRepo)
    {
        _connectionFactory = connectionFactory;
        _snapshotRepo      = snapshotRepo;
        _dripPoolRepo      = dripPoolRepo;
        _unclaimedPoolRepo = unclaimedPoolRepo;
    }

    public async Task<OneOf<NightlyJobResult, DomainError>> Handle(
        RunNightlyDripJobCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Dev/testing mode: scoped to the requested customer only
        var pendingSnapshots = (await _snapshotRepo.GetAllPendingBeforeDateAsync(today))
            .Where(s => s.CustomerId == request.CustomerId)
            .ToList();

        int forfeitsApplied = 0;

        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            // PASO 1 — Forfeit de ayer: mover allotment de DripPool a UnclaimedPool
            foreach (var snapshot in pendingSnapshots)
            {
                var dripPool = await _dripPoolRepo.GetByCustomerIdAsync(snapshot.CustomerId);

                if (dripPool is { Balance: > 0 })
                {
                    int allotment = (int)Math.Floor(dripPool.Balance * AllotmentPercentage);

                    if (allotment > 0)
                    {
                        await _unclaimedPoolRepo.AddToBalanceAsync(snapshot.CustomerId, allotment, tx);
                        await _dripPoolRepo.DeductBalanceAsync(snapshot.CustomerId, allotment, tx);
                    }
                }

                await _snapshotRepo.UpdateStatusAsync(snapshot.SnapshotId, ClaimStatus.Forfeited, null, tx);
                forfeitsApplied++;
            }

            // PASO 2 — Recarga del DripPool
            // TODO: wager grades processing — coordinated by existing LoyaltyPoints Job

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        return new NightlyJobResult(CustomersProcessed: 1, ForfeitsApplied: forfeitsApplied);
    }
}
