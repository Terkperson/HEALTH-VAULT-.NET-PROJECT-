# HealthVault

HealthVault is a .NET 8 healthcare management application built with ASP.NET Core and a clean architecture structure. The solution includes a web front end, a REST API, application services, domain models, and infrastructure code for persistence and integrations.

## Project overview

This repository contains a multi-project .NET solution under the `HealthVault/` folder:

- `HealthVault.Web` – ASP.NET Core web application
- `HealthVault.Api` – API layer for authentication and business endpoints
- `HealthVault.Application` – application logic and use cases
- `HealthVault.Domain` – core business entities and domain rules
- `HealthVault.Infrastructure` – data access, services, and infrastructure concerns

## Technology stack

- .NET 8
- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT authentication
- Clean Architecture layering

## Prerequisites

Before running the project locally, make sure you have:

- .NET 8 SDK installed
- SQL Server available for local development (or configured connection strings)
- A terminal with access to `dotnet`

## Getting started

From the repository root:

```bash
cd HealthVault

dotnet restore
dotnet build
```

### Run the API

```bash
dotnet run --project src/HealthVault.Api
```

### Run the web app

```bash
dotnet run --project src/HealthVault.Web
```

## Solution structure

```text
HEALTH-VAULT-.NET-PROJECT-
├── HealthVault/
│   ├── HealthVault.sln
│   └── src/
│       ├── HealthVault.Api/
│       ├── HealthVault.Application/
│       ├── HealthVault.Domain/
│       ├── HealthVault.Infrastructure/
│       └── HealthVault.Web/
├── README.md
├── README_SETUP.md
├── AZURE_AUTH_FIX.md
├── COMPLETE_SETUP_SUMMARY.md
└── azure-config-commands.ps1
```

## Configuration notes

The project includes Azure deployment and environment configuration guidance in the repository root docs:

- `README_SETUP.md`
- `AZURE_AUTH_FIX.md`
- `COMPLETE_SETUP_SUMMARY.md`

If you are deploying to Azure App Services, check those documents for the required app settings and auth configuration before publishing.

## Notes

This repository includes both local development setup instructions and Azure deployment troubleshooting notes for a production-style deployment scenario. Use the setup guides referenced above when working with the live environment.
