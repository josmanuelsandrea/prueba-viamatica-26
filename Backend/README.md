# ViamaticaApi — Backend

REST API for the Viamatica cash-register management system.

## Stack

- .NET 10 Web API
- PostgreSQL 17 (Npgsql + EF Core 10 + Dapper)
- JWT authentication with AES-256 encrypted claims
- Serilog (file + console sinks)
- Onion / DDD architecture

## Architecture

```
src/
  ViamaticaApi.Domain/          # Entities, domain interfaces
  ViamaticaApi.Application/     # Use cases, DTOs, FluentValidation
  ViamaticaApi.Infrastructure/  # EF Core DbContext, services, Dapper SPs
  ViamaticaApi.Api/             # Controllers, JWT middleware, Swagger
```

## Running locally

```bash
# Requires .NET 10 SDK and a running PostgreSQL 17 instance

cd Backend
dotnet build
dotnet run --project src/ViamaticaApi.Api/ViamaticaApi.Api.csproj
```

Swagger UI: `https://localhost:{port}/swagger`

### Connection string (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=viamatica;Username=viamatica_user;Password=viamatica_pass"
  },
  "Jwt": {
    "Key": "SuperSecretKey32CharactersMinimum!",
    "Issuer": "ViamaticaApi",
    "Audience": "ViamaticaClient"
  },
  "Encryption": {
    "Key": "AES256Key32BytesLongExactly!!!"
  }
}
```

## Running with Docker

```bash
# From the repository root
docker compose up --build
```

API will be available at `http://localhost:5000`.

## Stored procedure

The `sp_generate_turn_description` function is defined in `../db_init.sql`.

It generates turn numbers with the format `XX0001` (2-letter prefix + 4-digit counter, scoped to cash + attention type + day).

## Endpoints summary

| Method | Path | Description |
|--------|------|-------------|
| POST | /api/auth/login | Login |
| POST | /api/auth/recover-password | Password recovery |
| GET/POST/PUT/DELETE | /api/users | User management |
| GET/POST/PUT/DELETE | /api/clients | Client management |
| GET/POST/PUT/DELETE | /api/cashes | Cash register management |
| GET/POST | /api/turns | Turn management |
| GET/POST | /api/attentions | Attention records |
| GET/POST/PUT/DELETE | /api/services | Service catalogue |
| GET/POST | /api/contracts | Contract lifecycle |
| GET/POST | /api/payments | Payment records |
| GET/POST/POST | /api/kiosk | Public self-service kiosk |
| GET | /api/catalogs/* | Read-only catalogues |
