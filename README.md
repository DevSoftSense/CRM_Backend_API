# CRM_API — SoftOnCloud CRM backend

Same layered pattern as `Appointment/Appointment_API`.

## Solution

| Project | Role |
|---------|------|
| **CRM.API** | Controllers, JWT auth, Swagger, SignalR session hub, middleware |
| **CRM.Application** | Services (business rules) — call repositories only |
| **CRM.Domain** | DTOs / exceptions |
| **CRM.Infrastructure** | `ProductDatabaseHelper` + repositories — **call PostgreSQL `fn_*` only** (no inline SQL) |

Open: `CRM_Backend.slnx`

## Run (local)

```bash
cd CRM_API/CRM.API
dotnet run
```

- Swagger: http://localhost:5191/swagger  
- Health: http://localhost:5191/api/health  

### Login (SoftOnCloud Auth v2.5 — same flow as Appointment)

```http
POST /api/auth/login
{ "orgId": 100019, "email": "...", "password": "...", "rememberMe": true }
```

CRM_API calls SoftOnCloud `login` + `resolve-context?productId=28` (app-id from **config**, not browser).  
Returns JWT + `productContext` (roles / plan). Then: `Authorization: Bearer {token}`.

Also: `GET /api/auth/resolve-context`, `GET /api/auth/session-status`, `GET /api/auth/me`, session-renew, logout.  
Guide notes: `CRM_references/SOFTONCLOUD_AUTH_V25_CRM_INTEGRATION.md`.

## Config

| Key | Value |
|-----|-------|
| `CRM:AppId` / `SoftOnCloud:ProductId` | **28** (senior) |
| JWT Secret | SoftOnCloud platform secret (same as Appointment) |
| `SecondConnection` | Local `SOC_SaaS_Product` when `UseProductConnectionDb: false` |

Full auth + tenant explanation: `../CRM_references/CRM_AUTH_AND_TENANT_FLOW.md`.

## Rules

- Pass `p_org_id` (JWT) + `p_app_id` (config **28** via `CrmAppIdHelper`) into every `fn_*` — **never hardcode** in SQL bodies.
- Do not query `SOC_SaaS_V2` from product SQL — SoftOnCloud HTTP APIs only.
- Module controllers/services/repos are added as modules are built (Leads first).
