# LoyaltyDrip

Drip-pool mechanic for a loyalty points platform. Players accumulate points in a drip pool as wagers are graded; each day they can claim a percentage of that pool as spendable loyalty points. Unclaimed allotments are forfeited to a separate pool and never expire.

## Stack

- .NET 9 — Minimal API
- MediatR 12 — CQRS
- Dapper — SQL queries
- SQL Server
- OneOf — discriminated union results
- xUnit *(Phase 5 — pending)*

## Architecture

Clean Architecture with four projects:

```
LoyaltyPoints.Domain          — Entities, repository interfaces, IDbConnectionFactory
LoyaltyPoints.Application     — MediatR handlers, DomainError, DI registration
LoyaltyPoints.Infrastructure  — Dapper repositories, DbConnectionFactory, DI registration
LoyaltyPoints.API             — Minimal API endpoints, Program.cs
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/drip/claim` | Claim today's daily allotment |
| `GET` | `/drip/balances/{customerId}` | Fetch LP, drip pool, and unclaimed balances |
| `POST` | `/internal/drip/nightly-job` | Trigger nightly job for a single customer *(dev only)* |

### POST `/drip/claim`
```json
// Request
{ "customerId": "CUST000001" }

// Response 200
{ "allotmentCredited": 120 }

// Response 400 — already claimed today
// Response 404 — no active drip pool
```

### GET `/drip/balances/{customerId}`
```json
// Response 200
{
  "lpBalance": 4500,
  "dripPoolBalance": 600,
  "unclaimedBalance": 230
}
```

### POST `/internal/drip/nightly-job`
```json
// Request
{ "customerId": "CUST000001" }

// Response 200
{ "customersProcessed": 1, "forfeitsApplied": 0 }
```

## Business Rules

- Points enter the drip pool at **wager grade time**, not wager placement.
- The daily allotment is **DripPool.Balance × 20%**, calculated on-the-fly when the player claims.
- A player can claim **once per day**; the claim window closes when the 2 am nightly job runs.
- Unclaimed allotments are moved to the **UnclaimedPool** — they never expire.
- The nightly job runs in two steps: forfeit yesterday's unclaimed allotments, then reload the drip pool from graded wagers (Step 2 is coordinated by the existing LoyaltyPoints job).

## Nightly Job Cycle (2:00 am)

1. Find all `DailyClaimSnapshot` records with `Status = PENDING` and `CycleDate < today`.
2. For each: calculate `DripPool.Balance × 20%`, move that amount to `UnclaimedPool`, deduct from `DripPool`, mark snapshot `FORFEITED`.
3. *(Step 2 — wager grades processing coordinated by existing LoyaltyPoints Job)*

## Local Setup

### Prerequisites

- .NET 9 SDK
- SQL Server (local or Docker)

### Database

Run the DDL script against your local SQL Server instance:

```bash
# pending — sql/00_Dev_Database_Setup.sql (Phase 1 artifact)
```

### Connection String

Edit `LoyaltyPoints.API/appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=.;Database=LoyaltyDrip_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Run

```bash
dotnet run --project LoyaltyPoints.API
# API available at https://localhost:7195
```

## Development Phases

| Phase | Status | Description |
|-------|--------|-------------|
| 1 | ✅ Done | DDL + Domain entities |
| 2 | ✅ Done | Repository interfaces + Dapper implementations |
| 3 | ✅ Done | MediatR handlers |
| 4 | ✅ Done | Minimal API endpoints |
| 5 | 🔲 Pending | DriplConfig (ratios hardcoded for now) |
| 6 | 🔲 Pending | LoyaltyLedger (awaiting Finance definition) |
| 7 | 🔲 Pending | Unit + integration tests |

## Out of Scope (this iteration)

- VIP tiers
- Multi-currency
- LoyaltyLedger
- CRM campaigns (UnclaimedPool hook only)
- Cross-product point transfers
- Operator configuration UI
