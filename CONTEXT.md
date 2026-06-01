# Loyalty Drip Mechanic — Project Context
# Para Claude Code: lee este archivo primero antes de tocar cualquier otro.

## Stack
- .NET Core 9 Minimal API
- Clean Architecture
- MediatR
- Dapper
- SQL Server
- xUnit (testing)

---

## Decisiones confirmadas

### Timing y Job
- Los puntos del DripPool se asignan al momento del GRADEO de la apuesta, no al colocarla.
- Ya existe un Job nocturno de LoyaltyPoints que corre a las 2:00am todos los días.
- El Job del DripPool se piggybackea en ese mismo Job de las 2:00am.
- El Job de las 2:00am es el ÚNICO momento en que el DripPool crece (nuevas apuestas gradeadas).
- Durante el día el DripPool SOLO puede decrementar cuando el usuario hace Claim.
- NO existe un "snapshot congelado" del allotment — el allotment se calcula en tiempo real
  como (DripPool.Balance × porcentaje configurado) al momento en que el usuario abre la pantalla.
- El forfeit del día anterior también ocurre dentro del Job de las 2:00am, ANTES de calcular
  los nuevos puntos.

### Ciclo completo del Job (2:00am, orden de ejecución)
1. Buscar todos los DailyClaimSnapshot con Status=PENDING de ayer.
2. Para cada uno: mover el allotment a UnclaimedPool, marcar Status=FORFEITED.
3. Procesar las apuestas gradeadas del día: sumar puntos al DripPool de cada jugador.
4. (El siguiente allotment que verá el usuario = DripPool.Balance × % configurado, calculado on-the-fly)

### DailyClaimSnapshot — rol simplificado
- Ya NO almacena AllotmentAmount (se elimina esa columna — no hay nada que congelar).
- Su único propósito es PREVENIR EL DOBLE CLAIM.
- Estructura final: CustomerId | CycleDate | Status | ClaimedAt
- Un registro se crea al momento en que el usuario hace Claim (no antes).
- Si existe un registro CLAIMED para CustomerId + fecha de hoy → bloquear el claim.

### Otros
- Uncollected Points NO expiran. Se acumulan indefinidamente.
- Ventana de claim: cualquier hora del día hasta que el Job de las 2:00am corra.
- Status manejados como TINYINT en DB / enum en C#.
- CustomerId = VARCHAR(10).
- Balances en INT (sin decimales). crmLPTransactionBalances usa FLOAT (schema existente, no tocar).

---

## Preguntas aún abiertas (NO asumir respuesta)
- ¿Apuestas con fondos bonus generan DripPool?
- ¿Política de migración si cambian parámetros mid-pool?
- ¿Uncollected Points cuentan hacia VIP tiering?
- ¿Qué pasa si el Job de las 2am falla parcialmente? ¿Retry automático o manual?
- Estructura del LoyaltyLedger (pendiente con Finance)
- ¿El porcentaje del allotment diario es configurable desde ya o se hardcodea en esta iteración?

---

## Fuera de alcance (esta iteración)
- VIP tiers
- Multi-moneda
- LoyaltyLedger (pendiente)
- Campañas CRM (solo el hook de UnclaimedPool)
- Transferencia de puntos entre productos
- DriplConfig / configuración del operador (ratios hardcodeados por ahora)

---

# Estructura de la solución

LoyaltyDrip/
├── src/
│   ├── LoyaltyDrip.Api/                    # Proyecto de entrada — Minimal API
│   │   ├── Endpoints/
│   │   │   └── Drip/
│   │   │       ├── ClaimEndpoint.cs
│   │   │       └── GetBalancesEndpoint.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── LoyaltyDrip.Application/            # Casos de uso — MediatR Handlers
│   │   ├── Drip/
│   │   │   ├── Commands/
│   │   │   │   └── ClaimDailyAllotment/
│   │   │   │       ├── ClaimDailyAllotmentCommand.cs
│   │   │   │       └── ClaimDailyAllotmentHandler.cs
│   │   │   └── Queries/
│   │   │       └── GetCustomerBalances/
│   │   │           ├── GetCustomerBalancesQuery.cs
│   │   │           └── GetCustomerBalancesHandler.cs
│   │   └── Jobs/
│   │       └── NightlyDripJob/
│   │           ├── RunNightlyDripJobCommand.cs
│   │           └── RunNightlyDripJobHandler.cs
│   │
│   ├── LoyaltyDrip.Domain/                 # Entidades y contratos
│   │   ├── Entities/
│   │   │   ├── Customer.cs                 ✅ completo
│   │   │   ├── CrmLPTransactionBalances.cs ✅ completo
│   │   │   ├── DripPool.cs                 ✅ completo
│   │   │   ├── UnclaimedPool.cs            ✅ completo
│   │   │   └── DailyClaimSnapshot.cs       ✅ completo (ver nota abajo)
│   │   └── Repositories/
│   │       ├── ICustomerRepository.cs
│   │       ├── ILPBalanceRepository.cs
│   │       ├── IDripPoolRepository.cs
│   │       ├── IUnclaimedPoolRepository.cs
│   │       └── IDailyClaimSnapshotRepository.cs
│   │
│   └── LoyaltyDrip.Infrastructure/         # Implementaciones — Dapper + SQL Server
│       ├── Repositories/
│       │   ├── CustomerRepository.cs
│       │   ├── LPBalanceRepository.cs
│       │   ├── DripPoolRepository.cs
│       │   ├── UnclaimedPoolRepository.cs
│       │   └── DailyClaimSnapshotRepository.cs
│       ├── DependencyInjection.cs
│       └── DbConnectionFactory.cs
│
├── tests/
│   ├── LoyaltyDrip.Domain.Tests/
│   │   └── Drip/
│   │       ├── DripPoolTests.cs
│   │       ├── UnclaimedPoolTests.cs
│   │       └── DailyClaimSnapshotTests.cs
│   └── LoyaltyDrip.Application.Tests/
│       └── Drip/
│           ├── ClaimDailyAllotmentHandlerTests.cs
│           └── RunNightlyDripJobHandlerTests.cs
│
└── sql/
    └── 00_Dev_Database_Setup.sql            ✅ completo

---

# NOTA: DailyClaimSnapshot — ajuste pendiente en entidad y DDL

La entidad DailyClaimSnapshot.cs fue generada con un campo AllotmentAmount que ya NO se necesita.
Al arrancar el desarrollo, ajustar:

En la entidad (DailyClaimSnapshot.cs):
- Eliminar la propiedad AllotmentAmount
- Eliminar el parámetro allotmentAmount del constructor
- Simplificar: el snapshot solo registra que el claim ocurrió (CustomerId, CycleDate, Status, ClaimedAt)
- Un snapshot se crea ÚNICAMENTE cuando el usuario hace Claim exitoso — no antes
- El método MarkAsForfeited() sigue siendo válido (lo llama el Job de las 2am)

En el DDL (00_Dev_Database_Setup.sql):
- Eliminar la columna AllotmentAmount de la tabla DailyClaimSnapshot
- Eliminar el CHECK CONSTRAINT de AllotmentAmount
- El resto de la tabla queda igual

---

# Orden de desarrollo

## Fase 1 — COMPLETADO ✅
- [x] DDL SQL (sql/00_Dev_Database_Setup.sql)
- [x] Entidades de dominio (Domain/Entities/)

## Fase 2 — Repositorios (EMPEZAR AQUÍ) 🚀
Orden:
1. DbConnectionFactory                                  ← primero, todo depende de esto
2. ICustomerRepository + CustomerRepository
3. ILPBalanceRepository + LPBalanceRepository           (crmLPTransactionBalances)
4. IDripPoolRepository + DripPoolRepository
5. IUnclaimedPoolRepository + UnclaimedPoolRepository
6. IDailyClaimSnapshotRepository + DailyClaimSnapshotRepository
7. DependencyInjection.cs                               ← registrar todo en DI

## Fase 3 — Handlers (Application layer)
Orden estricto (hay dependencias entre ellos):
1. RunNightlyDripJobHandler    — el Job de las 2am (forfeit + recarga del DripPool)
2. ClaimDailyAllotmentHandler  — tap-to-claim del jugador
3. GetCustomerBalancesHandler  — query de los tres balances para el frontend

## Fase 4 — Endpoints (Api layer)
1. POST /drip/claim                  — ClaimDailyAllotmentCommand
2. GET  /drip/balances/{customerId}  — GetCustomerBalancesQuery
3. POST /internal/drip/nightly-job  — RunNightlyDripJobCommand (trigger del Job)

## Fase 5 — DriplConfig
Pendiente — ratios hardcodeados en esta iteración.

## Fase 6 — LoyaltyLedger
Pendiente — esperando definición de Finance.

---

# Interfaces de Repositorios esperadas

## IDripPoolRepository
- GetByCustomerIdAsync(customerId) → DripPool?
- GetAllWithBalanceAsync() → IEnumerable<DripPool>       ← Job de las 2am (procesar todos)
- UpsertAsync(dripPool) → void
- DeductBalanceAsync(customerId, amount) → void           ← al hacer Claim

## IDailyClaimSnapshotRepository
- GetByCustomerAndDateAsync(customerId, date) → DailyClaimSnapshot?   ← check doble-claim
- GetAllPendingBeforeDateAsync(date) → IEnumerable<DailyClaimSnapshot> ← Job forfeit
- InsertAsync(snapshot) → void                            ← al hacer Claim exitoso
- UpdateStatusAsync(snapshotId, status, claimedAt?) → void

## IUnclaimedPoolRepository
- GetByCustomerIdAsync(customerId) → UnclaimedPool?
- UpsertAsync(pool) → void
- AddToBalanceAsync(customerId, amount) → void            ← al forfeitear

## ILPBalanceRepository
- GetByCustomerIdAsync(customerId) → CrmLPTransactionBalances?
- AddPointsAsync(customerId, amount) → void               ← al hacer Claim

---

# Lógica del Job de las 2am (RunNightlyDripJobHandler)

El handler debe ejecutar estos pasos EN ORDEN dentro de una transacción:

```
PASO 1 — Forfeit de ayer
  - Obtener todos los DailyClaimSnapshot con Status=PENDING y CycleDate < hoy
  - Para cada uno:
      a. Calcular el allotment que se perdió: DripPool.Balance × porcentaje
      b. Mover ese monto a UnclaimedPool (AddToBalanceAsync)
      c. Restar ese monto del DripPool (DeductBalanceAsync)
      d. Marcar el snapshot como FORFEITED (UpdateStatusAsync)

PASO 2 — Recarga del DripPool
  - Obtener todas las apuestas gradeadas desde el último run del Job (pendientes de procesar)
  - Para cada apuesta:
      a. Calcular LP a agregar al DripPool (monto × ratio configurado)
      b. Sumar al DripPool del jugador (UpsertAsync / DeductBalanceAsync inverso)
  *** IMPORTANTE: este paso lo coordina el Job existente de LoyaltyPoints.
      Solo hay que agregar la llamada al DripPool dentro de ese Job. ***
```

Porcentaje del allotment (hardcodeado por ahora): 20%
Ratio DripPool por dólar apostado (hardcodeado por ahora): 1:1

---

# Lógica del Claim (ClaimDailyAllotmentHandler)

```
1. Verificar que exista un DripPool con Balance > 0 para el CustomerId.
2. Verificar que NO exista un DailyClaimSnapshot con Status=CLAIMED para hoy.
   → Si existe: retornar error "Ya reclamaste tu reward de hoy."
3. Calcular el allotment: DripPool.Balance × 20% (redondeado a INT).
4. Dentro de una transacción:
   a. Restar allotment del DripPool (DeductBalanceAsync)
   b. Acreditar allotment a crmLPTransactionBalances (AddPointsAsync)
   c. Insertar DailyClaimSnapshot con Status=CLAIMED y ClaimedAt=UtcNow
5. Retornar el allotment acreditado al frontend.
```

---

# Convenciones del proyecto
- Handlers retornan OneOf<TSuccess, Error> (patrón ya usado en el proyecto)
- Endpoints usan Results<Ok<T>, NotFound, ProblemHttpResult>
- Todos los timestamps en UTC (DateTime.UtcNow)
- SQL: sin ORM — queries Dapper planas en los repositorios
- Ningún script SQL se ejecuta desde código — solo se entregan para ejecución manual
- Sin comentarios en el código salvo XMLDoc en interfaces y entidades
