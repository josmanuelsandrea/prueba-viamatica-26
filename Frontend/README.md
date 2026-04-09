# ViamaticaFrontend — Frontend

Angular SPA for the Viamatica cash-register management system.

## Stack

- Angular 21 (NgModule + lazy loading, no standalone components)
- PrimeNG 21 + PrimeFlex + PrimeIcons
- Angular Signals (`signal`, `computed`, `effect`)
- Functional guards and interceptors
- ChangeDetectionStrategy.OnPush on all components

## Running locally

```bash
# Requires Node 20+

cd Frontend
npm install
npm start
```

App available at `http://localhost:4200`.

### Environment

Edit `src/environments/environment.ts` to change the API URL:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

## Running with Docker

```bash
# From the repository root
docker compose up --build
```

Frontend will be available at `http://localhost:4200`.
The nginx reverse proxy forwards `/api/*` to the backend container.

## Module structure

```
src/app/
  core/           # Auth service, interceptor, guards, API services
  shared/         # Navbar component, SharedModule
  auth/           # Login + password recovery (guest-guarded)
  welcome/        # Post-login landing (all roles)
  dashboard/      # Charts and indicators (admin only)
  admin/          # User management (admin only)
  gestor/         # Turn assignment (gestor only)
  cajero/         # Clients, contracts, payments (cajero only)
  kiosk/          # Public self-service kiosk (no auth required)
```

## Roles and access

| Role | Route | Features |
|------|-------|---------|
| Administrador (1) | /dashboard, /admin | User management, dashboard charts |
| Gestor (2) | /gestor/turnos | Assign turns to cashes |
| Cajero (3) | /cajero/clientes, /cajero/contratos, /cajero/pagos | Full cashier workflow |
| Public | /kiosk | Client self-service: identify, register, request turn |

## Kiosk

`/kiosk` is a public touchscreen-first interface where clients:
1. Enter their ID number via an on-screen numpad
2. Are identified or registered if new
3. Select an attention type
4. Receive a turn number with assigned cash module
