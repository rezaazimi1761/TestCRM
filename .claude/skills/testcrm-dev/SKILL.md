---
name: testcrm-dev
description: >
  Essential patterns and conventions for working in the TestCRM codebase — a
  multi-tenant ASP.NET Core 8 CRM with Blazor Server frontend. Use this skill
  whenever you are adding entities, writing CQRS handlers, building Blazor pages,
  or touching the API in this repo. Also trigger for any question about how
  paging, sorting, filtering, or multi-tenancy work here, or when adding new
  features to either TestCRM or CRM.Web projects.
---

# TestCRM Development Guide

## Solution at a glance

| Project | Role | Port |
|---|---|---|
| `Shared` | Common types: `BaseEntity`, `PagedResult<T>`, `ITenantService`, proto | — |
| `AuthService` | JWT issuance + gRPC token validation | 9041 |
| `TestCRM` | REST CRM API — CQRS / MediatR / EF Core 8 | 9040 |
| `CRM.Web` | Blazor Server UI — MudBlazor 6.20.0 | — |

---

## Multi-tenancy rules

Every entity inherits `BaseEntity` from `Shared/Domain/Common/BaseEntity.cs`:
```csharp
public abstract class BaseEntity
{
    public int    Id        { get; set; }
    public string TenantId  { get; set; } = string.Empty;
    public bool   IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

`AppDbContext` is registered **Scoped**. Its constructor captures `_currentTenant` once via `ITenantService`. `OnModelCreating` applies `HasQueryFilter` for every entity:
```csharp
entity.HasQueryFilter(e => e.TenantId == _currentTenant && !e.IsDeleted);
```

`SaveChangesAsync` stamps `TenantId` and `CreatedAt` on every new entity automatically — **never set these manually in commands**.

**Critical:** Never change `AppDbContext` lifetime to Singleton. The model is cached after first build, so the filter closure correctly captures the scoped `_currentTenant` only because a new context instance (and thus a new closure value) is created per request.

---

## Adding a new entity — complete checklist

1. `TestCRM/Domain/Entities/MyEntity.cs` — inherits `BaseEntity`
2. `AppDbContext` — add `DbSet<MyEntity>` property and `HasQueryFilter` block
3. `dotnet ef migrations add AddMyEntity` (from `TestCRM/` directory)
4. CQRS files under `TestCRM/Application/Features/MyEntity/`
5. `TestCRM/Controllers/MyEntityController.cs` with `[Authorize]`
6. `CRM.Web/Pages/MyEntity.razor` + `CRM.Web/Pages/MyEntityDialog.razor`
7. Nav link in `CRM.Web/Shared/MainLayout.razor`

---

## CQRS pattern

Each file holds **both** the record/class AND its handler. All return types use these conventions:

| Operation | Returns |
|---|---|
| `GetXxxsQuery` | `PagedResult<XxxDto>` |
| `GetXxxByIdQuery` | `XxxDto?` |
| `CreateXxxCommand` | `int` (new Id) |
| `UpdateXxxCommand` | `bool` |
| `DeleteXxxCommand` | `bool` (soft-delete only — set `IsDeleted = true`) |

### Query with paging + sorting + filtering (template)

```csharp
public record GetXxxsQuery(
    int Page = 1, int PageSize = 20,
    string? SortBy = null, bool SortDesc = false,
    string? Search = null
    // add entity-specific filters here, e.g.: string? Status = null
) : IRequest<PagedResult<Xxx>>;

public class GetXxxsQueryHandler : IRequestHandler<GetXxxsQuery, PagedResult<Xxx>>
{
    private readonly AppDbContext _db;
    public GetXxxsQueryHandler(AppDbContext db) => _db = db;

    public async Task<PagedResult<Xxx>> Handle(GetXxxsQuery r, CancellationToken ct)
    {
        var q = _db.Xxxs.AsQueryable();

        // search
        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var s = r.Search.ToLower();
            q = q.Where(x => x.Name.ToLower().Contains(s) /* add relevant fields */);
        }

        // enum / status filter
        // if (!string.IsNullOrWhiteSpace(r.Status) && Enum.TryParse<XxxStatus>(r.Status, out var st))
        //     q = q.Where(x => x.Status == st);

        var total = await q.CountAsync(ct);

        // sort — keys are lowercase (UI sends .ToLowerInvariant())
        q = r.SortBy switch
        {
            "fieldname" => r.SortDesc ? q.OrderByDescending(x => x.FieldName) : q.OrderBy(x => x.FieldName),
            _           => r.SortDesc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id),
        };

        var items = await q
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .ToListAsync(ct);   // or .Select(x => new XxxDto(...)) if projecting

        return new PagedResult<Xxx>(items, total, r.Page, r.PageSize);
    }
}
```

### Controller GET (template)

```csharp
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
    [FromQuery] string? sortBy = null, [FromQuery] bool sortDesc = false,
    [FromQuery] string? search = null
    // add [FromQuery] params for entity-specific filters
)
    => Ok(await _mediator.Send(new GetXxxsQuery(page, pageSize, sortBy, sortDesc, search)));
```

For entities with enum filters (like Tickets with `status` and `priority`), add matching `[FromQuery]` params and pass them to the query record.

---

## Blazor page pattern (server-side paging/sorting/filtering)

All list pages use `MudDataGrid<T>` with `ServerData`. Key rules:

### ⚠️ `SortBy` on columns is `Func<T, object?>`, NOT `string`

Never write `SortBy="fieldName"` — Razor will try to resolve it as a C# identifier and fail with CS0103.

| Column type | Sorting |
|---|---|
| `PropertyColumn` | **Remove `SortBy` entirely** — it auto-sorts by its `Property` lambda. The sort key sent to the server is the C# property name (e.g. `"FirstName"`). |
| `TemplateColumn` | Use a lambda: `SortBy="@(new Func<T,object?>(x => x.Status))"` |

The UI always calls `.ToLowerInvariant()` on the sort key before sending to the API, so query handler switch cases must use lowercase keys (`"firstname"`, `"status"`, etc.).

### Page structure template

```razor
@page "/xxxs"
@attribute [Authorize]
@inject CrmApiClient Crm
@inject ISnackbar Snackbar
@inject IDialogService DialogService

<!-- Search + filter toolbar -->
<MudStack Row="true" AlignItems="AlignItems.Center" Class="mb-3">
    <MudTextField @bind-Value="_search" Placeholder="Search…"
                  Adornment="Adornment.Start" AdornmentIcon="@Icons.Material.Filled.Search"
                  Immediate="true" DebounceInterval="350"
                  OnDebounceIntervalElapsed="OnSearchChanged"
                  Variant="Variant.Outlined" Margin="Margin.Dense" Style="min-width:240px" />
    <!-- Optional MudSelect for enum filters -->
    <MudButton Variant="Variant.Filled" Color="Color.Primary"
               StartIcon="@Icons.Material.Filled.Add" OnClick="@(() => OpenDialogAsync(null))" Class="ml-2">
        New Xxx
    </MudButton>
</MudStack>

<MudDataGrid T="Xxx" @ref="_grid" ServerData="LoadServerData"
             Hover="true" Dense="true" Striped="true" SortMode="SortMode.Single" Filterable="false">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <!-- TemplateColumn for enums: -->
        <!-- <TemplateColumn Title="Status" SortBy="@(new Func<Xxx,object?>(x => x.Status))"> -->
        <!--     <CellTemplate><MudChip>@context.Item.Status</MudChip></CellTemplate> -->
        <!-- </TemplateColumn> -->
        <TemplateColumn Title="Actions" Sortable="false">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" Color="Color.Primary" Size="Size.Small"
                               OnClick="@(() => OpenDialogAsync(context.Item))" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Error" Size="Size.Small"
                               OnClick="@(() => DeleteAsync(context.Item.Id))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    <PagerContent>
        <MudDataGridPager T="Xxx" PageSizeOptions="new[]{10,20,50,100}" />
    </PagerContent>
</MudDataGrid>

@code {
    private MudDataGrid<Xxx>? _grid;
    private string? _search;

    public class Xxx { public int Id { get; set; } /* ... */ }

    private async Task<GridData<Xxx>> LoadServerData(GridState<Xxx> state)
    {
        var sort = state.SortDefinitions.FirstOrDefault();
        try
        {
            var result = await Crm.GetPagedAsync<Xxx>(
                "/api/xxxs", state.Page + 1, state.PageSize,
                sort?.SortBy?.ToLowerInvariant(), sort?.Descending ?? false, _search);
            return new GridData<Xxx> { Items = result?.Items ?? new(), TotalItems = result?.TotalCount ?? 0 };
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Load failed: {ex.Message}", Severity.Error);
            return new GridData<Xxx> { Items = new(), TotalItems = 0 };
        }
    }

    private Task OnSearchChanged() => _grid?.ReloadServerData() ?? Task.CompletedTask;

    // ... OpenDialogAsync, DeleteAsync — call _grid?.ReloadServerData() on success
}
```

### Enum filter dropdown (add to toolbar for entities with status/priority)

```razor
<MudSelect T="string" Value="_statusFilter" Placeholder="All Statuses"
           Variant="Variant.Outlined" Margin="Margin.Dense" Clearable="true"
           Style="min-width:150px" Class="ml-2"
           ValueChanged="@((string? v) => { _statusFilter = v; _ = OnSearchChanged(); })">
    @foreach (var s in new[]{"New","Active","Closed"})
    { <MudSelectItem Value="@s">@s</MudSelectItem> }
</MudSelect>
```

Pass filter values to `GetPagedAsync` via the `filters` dictionary:
```csharp
var result = await Crm.GetPagedAsync<Xxx>(
    "/api/xxxs", state.Page + 1, state.PageSize,
    sort?.SortBy?.ToLowerInvariant(), sort?.Descending ?? false, _search,
    new Dictionary<string, string?> { ["status"] = _statusFilter });
```

---

## CrmApiClient — paging helper

`CrmApiClient.GetPagedAsync<T>` signature:
```csharp
Task<PagedResult<T>?> GetPagedAsync<T>(
    string path, int page, int pageSize,
    string? sortBy, bool sortDesc, string? search,
    Dictionary<string, string?>? filters = null)
```

It builds a query string like:
`/api/contacts?page=1&pageSize=20&sortBy=lastname&sortDesc=false&search=john`

---

## PagedResult<T>

Defined in `Shared/Application/Models/PagedResult.cs`:
```csharp
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

All `GetXxxsQuery` handlers return this. The UI reads `.Items` and `.TotalCount` to feed `GridData<T>`.

---

## Auth & Authorization

- JWT validation happens in `JwtAuthMiddleware` via gRPC call to AuthService per request
- All controllers must have `[Authorize]` on the class
- `ApiAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler` must be registered as singleton in `Program.cs` — without it, `[Authorize]` failures throw instead of returning 401
- Blazor pages use `@attribute [Authorize]`
- `AuthStateProvider` wraps all `localStorage` calls in `SafeGetAsync` to swallow `JSDisconnectedException` silently

---

## Dashboard charts (MudChart)

Chart data arrays **must** be pre-initialized to the exact length matching the labels array:
```csharp
// ✅ correct — matches the 4 enum values
_statusData = new double[4];

// ❌ crashes with IndexOutOfRangeException in MudBlazor legend rendering
_statusData = Array.Empty<double>();
```

---

## EF migrations

Always run from the `TestCRM/` project directory:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Migrations auto-run on startup via `Database.Migrate()` in `Program.cs`.
