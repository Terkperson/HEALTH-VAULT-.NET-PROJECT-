# HealthVault

2-week MVP of a clinic workspace for patients, staff and administrators.

Matches **SRS v1.1 (August 2026)**: authentication, patient management, appointments, medical records (upload / view / download), three dashboards, view-only admin. Record deletion and the admin role-reassignment UI are deferred.

## Stack

| Layer | Technology |
| --- | --- |
| Frontend | Blazor Web App, Interactive Server (.NET 8) |
| Backend | ASP.NET Core Web API |
| Language | C# |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Auth | ASP.NET Core Identity + JWT |
| UI | Bootstrap 5 |

Blazor Server is the Section 5 fallback: same C# / EF / Identity / Azure stack as the original WASM proposal, without the Day-1 WASM/CORS tax.

## Solution layout

```
HealthVault/
  HealthVault.sln
  database/                     SQL Server scripts for SSMS / Azure Data Studio
  docs/HealthVault_Database.html
  docs/HealthVault_Tables.xlsx
  src/HealthVault.Domain        Entities and enums
  src/HealthVault.Application   DTOs and service contracts
  src/HealthVault.Infrastructure Identity, EF Core, services
  src/HealthVault.Api           REST + JWT + Swagger
  src/HealthVault.Web           Blazor UI
```

## 1. Create the database in a workbench

Use SSMS, Azure Data Studio, or any SQL Server workbench.

1. Connect to your instance.
2. Run `database/01_CreateDatabase.sql`
3. Run `database/02_CreateTables.sql`
4. Run `database/03_SeedData.sql`
5. Keep `database/04_AssignRole.sql` for MVP role changes (FR-20).

Full column catalog and ERD: open `docs/HealthVault_Database.html`.

## 2. Point the API at SQL Server

Edit `src/HealthVault.Api/appsettings.json`:

Windows authentication:

```
Server=localhost;Database=HealthVaultDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

SQL login (typical on a shared lab machine):

```
Server=localhost;Database=HealthVaultDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=True
```

If the database already exists from the scripts, the API will not recreate tables. It will only seed Identity users (passwords must be hashed by Identity, so they are not inserted in T-SQL).

## 3. Run

```bash
dotnet restore
dotnet run --project src/HealthVault.Api
dotnet run --project src/HealthVault.Web
```

- API / Swagger: http://localhost:5080/swagger
- Web UI: http://localhost:5081

If the Web app is on another host, set `ApiBaseUrl` in `src/HealthVault.Web/appsettings.json`.

## 4. Deploy the API and Web app

This is a separate frontend and backend project, so submit one GitHub link and one deployment link for each app.

Deploy `src/HealthVault.Api` and `src/HealthVault.Web` as two .NET 8 web services. The hosting platform must provide a reachable SQL Server database. Run the four scripts in `database/` against that database before opening the API URL; the API then creates the demo Identity accounts on first start.

Set these environment variables on the API service:

```
ConnectionStrings__DefaultConnection=<SQL Server connection string>
Jwt__Key=<long random production secret>
Jwt__Issuer=HealthVault
Jwt__Audience=HealthVault.Clients
```

Set this environment variable on the Web service, using the public API URL and a trailing slash:

```
ApiBaseUrl=https://<your-api-domain>/
```

The apps use the hosting platform's `PORT` variable when it is supplied, and keep ports 5080 and 5081 for local development. Do not commit production connection strings, JWT keys, or real patient data.

## Demo accounts

Created on first successful API start.

| Role | Email | Password |
| --- | --- | --- |
| Administrator | admin@healthvault.com | Admin@12345 |
| Staff | staff@healthvault.com | Staff@12345 |
| Staff | nurse@healthvault.com | Staff@12345 |
| Patient | patient@healthvault.com | Patient@12345 |

## Core workflow (success criteria)

Register / sign in → book appointment → staff confirms → upload record → patient views / downloads.

## Out of scope (unchanged from the SRS)

SignalR, SMS/email, Azure Blob, Key Vault, video consults, FHIR, mobile app, automated CI/CD, production HIPAA.

## Role changes in the MVP

There is no role-edit screen. Update `database/04_AssignRole.sql` (`@Email`, `@NewRole`) and execute it.
