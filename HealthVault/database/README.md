# HealthVault — SQL Server scripts

Run these in **SSMS**, **Azure Data Studio**, or any SQL Server workbench, in this order:

| # | File | What it does |
| --- | --- | --- |
| 1 | `01_CreateDatabase.sql` | Creates `HealthVaultDb` |
| 2 | `02_CreateTables.sql` | 16 tables, FKs, indexes, two views |
| 3 | `03_SeedData.sql` | Genders, blood groups, statuses, departments, categories, roles |
| 4 | `04_AssignRole.sql` | MVP role change (no UI — SRS FR-20) |
| 5 | `05_UsefulQueries.sql` | Directory, board, double-booking check, admin counts |

Human-readable catalog:

- `docs/HealthVault_Database.html` — ERD + every column
- `docs/HealthVault_Tables.xlsx` — same catalog as a workbook (Overview, inventory, columns, relationships, indexes, seed, connection strings)

Do **not** insert demo users in T-SQL. Start `HealthVault.Api` once so Identity can write salted password hashes.
