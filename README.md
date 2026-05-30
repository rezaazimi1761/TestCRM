# 🏢 TestCRM — Multi-Tenant CRM Platform

> A production-ready, multi-tenant CRM backend built with **ASP.NET Core 8**, **Entity Framework Core**, **CQRS / MediatR**, and a dedicated **Auth microservice** that communicates over **gRPC**.

---

## 📐 Solution Architecture

```
TestCRM.sln
├── 📦 Shared/              ← Class library — shared contracts, proto, base types
├── 🔐 AuthService/         ← JWT auth microservice  (port 9041)
└── 🗂️  TestCRM/            ← CRM API service         (port 9040)
```

```
┌─────────────────────────────────────────────────────────────┐
│                        Client / Frontend                     │
└────────────────┬────────────────────┬───────────────────────┘
                 │  REST              │  REST
                 ▼                   ▼
        ┌────────────────┐   ┌──────────────────┐
        │  AuthService   │   │   CRM API        │
        │  :9041         │   │   :9040          │
        │                │   │                  │
        │  • Register    │   │  • Contacts      │
        │  • Login       │◄──│  • Accounts      │  gRPC
        │  • Refresh     │   │  • Leads         │  ValidateToken
        │  • Tenants     │   │  • Opportunities │  GetUserById
        │  • Claims      │   │  • Activities    │  GetUserClaims
        │  • Switch      │   │  • Users         │
        └───────┬────────┘   └────────┬─────────┘
                │                     │
                ▼                     ▼
        ┌──────────────┐     ┌──────────────────┐
        │  SQL Server  │     │   SQL Server     │
        │  Auth DB     │     │   CRM DB         │
        └──────────────┘     └──────────────────┘
```

---

## 🧩 Projects

### `Shared` — Class Library

Single source of truth for everything used by more than one service.

| Path | Contents |
|------|----------|
| `Protos/auth.proto` | gRPC contract — compiled **GrpcServices="Both"** |
| `Domain/Common/BaseEntity.cs` | Base class with `Id`, `TenantId`, `CreatedAt`, `UpdatedAt`, `IsDeleted` |
| `Application/Interfaces/ITenantService.cs` | Tenant-resolution interface |
| `Infrastructure/Services/TenantService.cs` | Reads `tenant_id` claim → `X-Tenant-Id` header → `"default"` |
| `Contracts/Auth/AuthContracts.cs` | Request / response records for auth endpoints |
| `Contracts/Tenant/TenantContracts.cs` | Request / response records for tenant endpoints |

---

### `AuthService` — Authentication Microservice (`:9041`)

#### REST API

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/auth/register` | Public | Create a new user under a tenant |
| `POST` | `/api/auth/login` | Public | Returns `AccessToken` + `RefreshToken` |
| `POST` | `/api/auth/refresh` | Public | Rotate refresh token |
| `POST` | `/api/auth/revoke` | 🔒 Any | Revoke a refresh token |
| `GET`  | `/api/auth/me` | 🔒 Any | Dump current user's claims |
| `POST` | `/api/auth/switch-tenant` | 🔒 **SuperUser** | Issue a new JWT scoped to a different tenant |
| `GET`  | `/api/tenants` | 🔒 Any | List tenants (SuperUser sees inactive too) |
| `GET`  | `/api/tenants/{slug}` | 🔒 Any | Get single tenant |
| `POST` | `/api/tenants` | 🔒 **SuperUser** | Create tenant |
| `PUT`  | `/api/tenants/{slug}` | 🔒 **SuperUser** | Update tenant |
| `DELETE` | `/api/tenants/{slug}` | 🔒 **SuperUser** | Soft-delete tenant + cascade users |
| `PATCH` | `/api/tenants/{slug}/activate` | 🔒 **SuperUser** | Re-activate tenant |
| `PATCH` | `/api/tenants/{slug}/deactivate` | 🔒 **SuperUser** | Deactivate tenant |
| `GET`  | `/api/users/{id}/claims` | 🔒 Any | List user's custom claims |
| `POST` | `/api/users/{id}/claims` | 🔒 Admin/SuperUser | Add a claim |
| `PUT`  | `/api/users/{id}/claims` | 🔒 Admin/SuperUser | Replace all claims |
| `DELETE` | `/api/users/{id}/claims/{claimId}` | 🔒 Admin/SuperUser | Remove a claim |

#### gRPC API (consumed internally by CRM)

```protobuf
service AuthGrpc {
  rpc ValidateToken   (ValidateTokenRequest)   returns (ValidateTokenResponse);
  rpc GetUserById     (GetUserByIdRequest)      returns (UserResponse);
  rpc GetUserClaims   (GetUserClaimsRequest)    returns (UserClaimsResponse);
  rpc GetTenantBySlug (GetTenantBySlugRequest)  returns (TenantResponse);
}
```

#### Domain Entities

```
AppUser ──── RefreshToken
    └─────── UserClaim
Tenant ────► AppUser  (FK: Tenant.Slug → AppUser.TenantId)
```

---

### `TestCRM` — CRM API (`:9040`)

Full CRUD for all 6 CRM entities via **CQRS + MediatR**.

| Entity | Endpoints | Key Fields |
|--------|-----------|------------|
| **User** | `GET /api/users` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | FirstName, LastName, Email, Role, IsActive |
| **Contact** | `GET /api/contacts` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | FirstName, LastName, Email, Phone, Company, JobTitle |
| **Account** | `GET /api/accounts` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | Name, Industry, Website, Phone, Address |
| **Lead** | `GET /api/leads` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | Name, Source, Status (New/Contacted/Qualified/Lost) |
| **Opportunity** | `GET /api/opportunities` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | Title, Value, Stage, ExpectedCloseDate |
| **Activity** | `GET /api/activities` · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` | Subject, Type, DueDate, IsCompleted |

---

## 🔒 Multi-Tenancy

Every entity inherits `BaseEntity` which carries a `TenantId` column.

```csharp
// Automatically applied to EVERY entity via EF Global Query Filters
modelBuilder.Entity<T>().HasQueryFilter(e =>
    e.TenantId == _currentTenant && !e.IsDeleted);

// TenantId is stamped on every INSERT automatically in SaveChangesAsync
entry.Entity.TenantId = _currentTenant;
```

**Tenant resolution order** (per HTTP request):

```
JWT claim "tenant_id"  →  X-Tenant-Id header  →  "default"
```

### 🦸 SuperUser Tenant Switch

A user with role `SuperUser` can impersonate any tenant without re-authenticating:

```
POST /api/auth/switch-tenant
{ "targetTenantSlug": "acme" }

→ Returns a new JWT where:
     tenant_id      = "acme"        ← active scope
     home_tenant_id = "super-corp"  ← real home, never changes
     tenant_switched = true
```

The CRM service reads the `tenant_id` claim transparently — EF filters apply to the switched tenant automatically.

---

## 🔑 JWT Token Claims

```json
{
  "sub":              "42",
  "email":            "reza@example.com",
  "unique_name":      "reza",
  "role":             "SuperUser",
  "tenant_id":        "acme",
  "home_tenant_id":   "super-corp",
  "tenant_switched":  "true",
  "first_name":       "Reza",
  "last_name":        "Smith",
  "jti":              "<uuid>",
  "exp":              1234567890
}
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8 / ASP.NET Core 8 |
| ORM | Entity Framework Core 8 + SQL Server |
| Auth | JWT Bearer (HS256) + Refresh Tokens |
| Service Communication | gRPC (Grpc.AspNetCore 2.62) |
| CQRS | MediatR 12 |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swashbuckle / Swagger UI |
| Database | Microsoft SQL Server |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server instance (or Docker: `docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Abc1234@$' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`)

### Configuration

**`AuthService/appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<host>;Database=CRMAuth;User Id=sa;Password=<pwd>;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret":               "YourSuperSecretKeyMustBe32CharsMinimum!!",
    "Issuer":               "AuthService",
    "Audience":             "CRMServices",
    "AccessTokenMinutes":   "60",
    "RefreshTokenDays":     "7"
  }
}
```

**`TestCRM/appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<host>;Database=CRM;User Id=sa;Password=<pwd>;TrustServerCertificate=True;"
  },
  "AuthService": {
    "GrpcUrl": "http://localhost:9041"
  }
}
```

### Run

Databases are migrated automatically on startup.

```bash
# Terminal 1 — Auth microservice
cd AuthService
dotnet run
# → http://localhost:9041  (REST + gRPC)

# Terminal 2 — CRM API
cd TestCRM
dotnet run
# → http://localhost:9040
```

Swagger UI: `http://localhost:9041/swagger` · `http://localhost:9040/swagger`

---

## 📁 Repository Structure

```
TestCRM.sln
│
├── Shared/
│   ├── Protos/auth.proto
│   ├── Domain/Common/BaseEntity.cs
│   ├── Application/Interfaces/ITenantService.cs
│   ├── Infrastructure/Services/TenantService.cs
│   └── Contracts/
│       ├── Auth/AuthContracts.cs
│       └── Tenant/TenantContracts.cs
│
├── AuthService/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── ClaimsController.cs
│   │   └── TenantsController.cs
│   ├── Domain/Entities/
│   │   ├── AppUser.cs
│   │   ├── Tenant.cs
│   │   ├── RefreshToken.cs
│   │   └── UserClaim.cs
│   ├── Infrastructure/Persistence/AuthDbContext.cs
│   ├── Services/
│   │   ├── JwtTokenService.cs
│   │   ├── ClaimManagerService.cs
│   │   └── AuthGrpcService.cs
│   └── Migrations/
│
└── TestCRM/
    ├── Controllers/          (Users, Contacts, Accounts, Leads, Opportunities, Activities)
    ├── Domain/Entities/
    ├── Application/Features/ (CQRS Commands + Queries per entity)
    ├── Infrastructure/
    │   ├── Persistence/AppDbContext.cs
    │   ├── GrpcClients/AuthGrpcClient.cs
    │   └── Middleware/JwtAuthMiddleware.cs
    └── Migrations/
```

---

## 🔄 Request Flow

```
1. Client  →  POST /api/auth/login  →  AuthService
2. AuthService returns { accessToken, refreshToken }

3. Client  →  GET /api/contacts  (Bearer <token>)  →  TestCRM

4. JwtAuthMiddleware intercepts
          →  gRPC ValidateToken(token)  →  AuthService
          ←  { isValid, userId, role, tenantId }

5. ClaimsPrincipal injected into HttpContext

6. TenantService reads "tenant_id" claim  →  "acme"

7. AppDbContext applies EF Query Filter:
          WHERE TenantId = 'acme' AND IsDeleted = 0

8. Response returned to client
```

---

## 👤 User Roles

| Role | Permissions |
|------|-------------|
| `User` | Read / write own-tenant data |
| `Admin` | + Manage claims for users in own tenant |
| `SuperUser` | + Create/manage tenants, switch to any tenant, create SuperUser accounts |

---

*Built with ❤️ using .NET 8 · EF Core · gRPC · MediatR · JWT*
