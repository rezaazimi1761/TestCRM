---
name: modern-crm-development
description: Develop, review, debug, and extend the ModernCRM .NET 8 solution while preserving its Tactical DDD, CQRS, multi-tenant, EF Core, JWT, MassTransit, RabbitMQ, and Blazor Server conventions. Use for changes to the Auth or CRM bounded contexts, shared integration events, persistence and migrations, APIs, user synchronization saga/outbox flow, or the MudBlazor web UI in this repository.
---

# ModernCRM Development

## Start with context

1. Read the root `AGENTS.md` for commands, security constraints, and repository-wide rules.
2. Read `ARCHITECTURE.md` and [references/project-map.md](references/project-map.md) when a change crosses a layer or bounded context.
3. Inspect the nearest existing aggregate, handler, repository, controller, client, or Razor page and follow its established pattern.
4. Check `git status --short` before editing and preserve unrelated user changes.

## Place changes correctly

- Put invariants and state transitions in Domain aggregates and value objects.
- Put use-case orchestration in Application commands, queries, handlers, and DTOs.
- Put EF Core, repository implementations, identity, and messaging adapters in Infrastructure.
- Put HTTP contracts, controllers, composition, consumers, and saga hosting in Api. Do not place or inject a DbContext in Api.
- Put reusable cross-context primitives and integration contracts in SharedKernel; do not move context-specific domain behavior there.
- Put UI components, pages, auth state, and typed API clients in `ModernCRM.Web`.

Keep dependencies directed inward: Api/Infrastructure -> Application -> Domain -> SharedKernel. Do not reference one bounded context's Domain or Infrastructure project from the other context.

## Implement a domain use case

1. Identify the owning bounded context and aggregate.
2. Add or update an aggregate method that enforces the business rule and raises a domain event when the transition matters.
3. Add the command/query and DTO in Application.
4. Add a handler using the repository abstraction; keep controller logic thin.
5. Update the repository interface and EF implementation only when persistence needs change.
6. Add or adjust API contracts and controller endpoints.
7. Register new handlers or services in the owning API `Program.cs`.
8. Update the Blazor API client and page only when the feature is user-facing.

## Change cross-context user synchronization

Treat Auth as owner of users and CRM's `CrmUserReference` as a local projection. Define published contracts in `SharedKernel/IntegrationEvents`. Preserve MassTransit retry, EF outbox/inbox, and saga correlation behavior. Make consumers idempotent because delivery can repeat. Never share an Auth database model directly with CRM.

## Change persistence

Keep every DbContext, EF registration, migration, and database initializer in Infrastructure. Expose persistence through repositories and commit through each repository's Unit of Work; never call a DbContext from Api or Application. Update entity configuration in the owning DbContext and repository together. Generate migrations in the Infrastructure project that owns the DbContext. Review generated migrations and snapshots before accepting them. Do not hand-edit snapshots unless repairing a known migration issue.

## Validate

Run the narrowest useful checks first, then run:

```powershell
dotnet restore ModernCRM.sln --configfile NuGet.config
dotnet build ModernCRM.sln --no-restore
```

Run relevant tests if test projects exist. For runtime verification, ensure SQL Server and RabbitMQ are available, then start Auth API on `9041`, CRM API on `9040`, and Web on `9050`. Report when infrastructure prevents runtime verification; do not claim it passed.

## Protect configuration

Do not add credentials, JWT secrets, or production endpoints to tracked appsettings files. Use user secrets or environment variables for new sensitive values. Required runtime secrets include `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, and `RabbitMQ__Password`; initial Auth seeding additionally requires `Seed__AdminPassword`. Do not echo existing secrets in logs, patches, or summaries. Keep JWT issuer, audience, and signing key consistent across Auth and CRM. Apply migrations during deployment, or explicitly opt in to startup migration with `Database__ApplyMigrationsOnStartup=true`.
