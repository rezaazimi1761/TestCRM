# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Solution Structure

Four projects in `TestCRM.sln`:

| Project | Type | Port | Role |
|---|---|---|---|
| `Shared` | Class library | — | Shared interfaces, base entities, infrastructure services |
| `AuthService` | ASP.NET Core 8 | 9041 | Authentication, user/tenant management, gRPC token validation |
| `TestCRM` | ASP.NET Core 8 | 9040 | CRM REST API (CQRS/MediatR, EF Core 8, SQL Server) |
| `CRM.Web` | Blazor Server | — | Frontend UI (MudBlazor 6.20.0) |

## Running the Services

Each service must run simultaneously. Open two terminals:

```bash
# Terminal 1
cd AuthService && dotnet run

# Terminal 2
cd TestCRM && dotnet run
```

EF Core migrations run automatically on startup (`Database.Migrate()`). To create a new migration manually:

```bash
cd TestCRM
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Architecture

### Multi-Tenancy
Every entity inherits `BaseEntity` (from `Shared/Domain/Common/`) which has `TenantId`, `IsDeleted`, `CreatedAt`, `UpdatedAt`.

**`AppDbContext`** is `Scoped`. In its constructor it captures `_currentTenant` from `ITenantService`. `HasQueryFilter` on every entity filters by `_currentTenant && !IsDeleted`. `SaveChangesAsync` stamps `TenantId` on new entities.

**`TenantService`** (in `Shared/Infrastructure/Services/`) resolves tenant in this order:
1. JWT claim `tenant_id` (injected by `JwtAuthMiddleware`)
2. `X-Tenant-Id` request header
3. Falls back to `"default"`

**⚠️ Known EF Core caveat**: `OnModelCreating` executes once per application lifetime and the model is cached. `HasQueryFilter` captures `_currentTenant` from the constructor field — this works correctly only because `AppDbContext` is registered as `Scoped` (new instance per request), so each request creates a new `AppDbContext` with a fresh `_currentTenant`. Do not change the lifetime to Singleton.

### Authentication Flow
1. Blazor frontend calls AuthService `/api/auth/login` → receives JWT
2. JWT is stored in browser `localStorage` via `Blazored.LocalStorage`
3. Every CRM API call includes `Authorization: Bearer <token>` header
4. `JwtAuthMiddleware` in TestCRM validates the token by calling `AuthService` via gRPC
5. On success, it sets `HttpContext.User` with claims including `tenant_id`
6. Controllers use `[Authorize]` attribute

`ApiAuthorizationResultHandler` (implements `IAuthorizationMiddlewareResultHandler`) must be registered to return proper 401/403 without requiring `AddAuthentication()`. Without it, `[Authorize]` failures throw an exception instead of returning 401.

Register in `Program.cs`:
```csharp
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>();
```

### CQRS Pattern
All business logic lives in `TestCRM/Application/Features/{Entity}/`:
- `Commands/CreateXxxCommand.cs` — creates entity, returns `int` (new Id)
- `Commands/UpdateXxxCommand.cs` — updates entity, returns `bool`
- `Commands/DeleteXxxCommand.cs` — soft-deletes entity, returns `bool`
- `Queries/GetXxxsQuery.cs` — returns `List<XxxDto>`
- `Queries/GetXxxByIdQuery.cs` — returns `XxxDto?`

Each file contains both the record/class command/query and its `IRequestHandler` implementation.

### Enum Serialization
`AddJsonOptions` in `Program.cs` registers `JsonStringEnumConverter` so enums serialize as strings in JSON. When adding new enum properties, no extra configuration is needed.

## Adding a New Entity — Checklist

1. **Domain**: Add `XxxEntity.cs` in `TestCRM/Domain/Entities/` inheriting `BaseEntity`
2. **DbContext**: Add `public DbSet<Xxx> Xxxs => Set<Xxx>();` and `HasQueryFilter` in `AppDbContext`
3. **EF Migration**: `dotnet ef migrations add AddXxx` from the `TestCRM` directory
4. **CQRS**: Add Commands (Create/Update/Delete) and Queries (GetAll/GetById) under `Application/Features/Xxx/`
5. **Controller**: Add `XxxController.cs` in `Controllers/` with `[Authorize]`
6. **Blazor**: Add `XxxDialog.razor` (MudDialog) and `Xxx.razor` (page) under `CRM.Web/Pages/`
7. **Nav**: Add `MudNavLink` in `CRM.Web/Shared/MainLayout.razor`

## Blazor (CRM.Web) Notes

- **Auth state**: `CRM.Web/Auth/AuthStateProvider.cs` wraps `localStorage` calls in `SafeGetAsync` that swallows `JSDisconnectedException`, `JSException`, and `InvalidOperationException` (Blazor circuit lifecycle).
- **API client**: `CRM.Web/Services/CrmApiClient.cs` builds `HttpClient` per call using stored token + API URL.
- **Dashboard charts**: Uses `MudChart ChartType.Donut`. Chart data arrays must be pre-initialized to the correct length (e.g., `new double[4]`) to avoid `IndexOutOfRangeException` in MudBlazor's legend rendering.
- **UI library**: MudBlazor 6.20.0 — use `MudDialog`, `MudDataGrid`, `MudSelect`, `MudDatePicker` for consistent UI.
