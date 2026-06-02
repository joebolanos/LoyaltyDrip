-- ============================================================
-- LoyaltyDrip - DB Schema: Script Maestro
-- Ejecutar en SSMS con SQLCMD Mode activado
-- Query > SQLCMD Mode
-- ============================================================
PRINT '>>> [00] Drop objects...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\00_Drop.sql"
PRINT '>>> [01] Table: Customer...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\01_Table_Customer.sql"
PRINT '>>> [02] Table: crmLPTransactionBalances...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\02_Table_crmLPTransactionBalances.sql"
PRINT '>>> [03] Table: DripConfig...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\03_Table_DripConfig.sql"
PRINT '>>> [04] Table: crmLPTransactions...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\04_Table_crmLPTransactions.sql"
PRINT '>>> [05] Table: DripPool...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\05_Table_DripPool.sql"
PRINT '>>> [06] Table: UnclaimedPool...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\06_Table_UnclaimedPool.sql"
PRINT '>>> [07] Table: DailyClaimSnapshot...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\07_Table_DailyClaimSnapshot.sql"
PRINT '>>> [08] SP: GetDailyAllotmentPreview...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\08_SP_GetDailyAllotmentPreview.sql"
PRINT '>>> [09] SP: ClaimDailyAllotment...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\09_SP_ClaimDailyAllotment.sql"
PRINT '>>> [10] SP: ProcessNightlyDripJob...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\10_SP_ProcessNightlyDripJob.sql"
PRINT '>>> [11] SP: GetLoyaltyPointsBalanceByUser...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\11_SP_GetLoyaltyPointsBalanceByUser.sql"
PRINT '>>> [12] Seeds...'
:r "C:\Users\joebo\Documents\LoyaltyDrip - DB Schema\12_Seeds.sql"
PRINT '>>> ✅ Todos los scripts ejecutados correctamente.'