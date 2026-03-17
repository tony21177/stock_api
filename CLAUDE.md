# CLAUDE.md — stock_api

## Project Overview
Warehouse/inventory management REST API for healthcare stock operations. Handles purchases, stock in/out, QC, inventory adjustments, supplier traceability, and notifications.

## Tech Stack
- **Framework**: ASP.NET Core Web API (.NET 6.0)
- **Language**: C# with nullable and implicit usings enabled
- **Database**: MySQL 8.1 via Entity Framework Core 7.0 (Pomelo provider)
- **Auth**: JWT Bearer authentication with role-based authorization
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Scheduling**: Quartz.NET (notification jobs)
- **Logging**: Serilog (console + daily rolling files in /logs/)
- **Email**: MailKit (SMTP via Gmail)

## Architecture
3-tier: Controllers → Services → Models (EF Core entities)
- `stock_api/Controllers/` — API endpoints + Request DTOs + Validators
- `stock_api/Service/` — Business logic + Value Objects
- `stock_api/Models/` — EF Core entities (auto-generated via EF Core Power Tools) + StockDbContext
- `stock_api/Common/` — DI config, constants, settings, AutoMapper profiles, utilities
- `stock_api/Auth/` — JWT auth, role-based authorization
- `stock_api/Middleware/` — Permission filter, request logging
- `stock_api/Scheduler/` — Quartz background jobs for notifications

## Build & Run
```bash
# Build (publish self-contained Windows x64)
./build.bat
# Output: ./output/net6.0/win-x64/stock_api.exe

# Run locally
dotnet run --project stock_api
# Kestrel listens on http://localhost:37189
```

## Naming Conventions
- Controllers: `{Entity}Controller.cs`
- Services: `{Entity}Service.cs`
- Request DTOs: `{Action}{Entity}Request.cs` (in Controllers/Request/)
- Value Objects: `{Entity}Vo.cs` (in Service/ValueObject/)
- Validators: `{Entity}Validator.cs` (in Controllers/Validator/)

## Commit Message Style
Use prefix tags in brackets, written in Chinese:
- `[feat]` — new feature
- `[fix]` — bug fix
- Example: `[feat] 分成_驗收品項列表_和_入庫品項列表`

## Key Conventions
- DI registration in `Common/IoC.Configuration/DI/ServiceCollectionExtensions.cs`
- Response compression enabled (Brotli + Gzip)
- Database entities are auto-generated — do not manually edit `StockDbContext.cs`
- App configuration in `appsettings.json` (JWT, DB connection, SMTP, Kestrel)
