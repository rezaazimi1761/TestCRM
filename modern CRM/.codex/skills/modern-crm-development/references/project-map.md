# ModernCRM project map

## Solution

- Target framework: .NET 8
- UI: Blazor Server with MudBlazor and browser local storage
- Persistence: EF Core with SQL Server
- Authentication: JWT bearer tokens issued by Auth
- Messaging: MassTransit with RabbitMQ, EF outbox/inbox, and a CRM-hosted saga

## Projects

| Area | Project | Responsibility |
| --- | --- | --- |
| Shared | `src/SharedKernel/ModernCRM.SharedKernel` | DDD building blocks, common value objects, integration event contracts |
| Auth | `ModernCRM.Auth.Domain` | AuthUser, Tenant, Role, Claim, credential value objects and domain events |
| Auth | `ModernCRM.Auth.Application` | Auth commands, queries, handlers, DTOs, application ports |
| Auth | `ModernCRM.Auth.Infrastructure` | Auth DbContexts, repositories, Unit of Work, migrations, database initialization, password hashing, JWT issuing, publisher |
| Auth | `ModernCRM.Auth.Api` | Auth HTTP endpoints, composition root and integration consumer; no direct DbContext access |
| CRM | `ModernCRM.Crm.Domain` | Account, Contact, Ticket, Opportunity, CrmUserReference |
| CRM | `ModernCRM.Crm.Application` | CRM commands, queries, handlers, DTOs |
| CRM | `ModernCRM.Crm.Infrastructure` | CRM DbContexts, repositories, Unit of Work, migrations, database initialization, current-tenant provider and integration persistence |
| CRM | `ModernCRM.Crm.Api` | CRM endpoints, composition root, integration consumers and saga orchestration; no direct DbContext access |
| UI | `src/Presentation/ModernCRM.Web` | Blazor pages, dialogs, auth state and API clients |

## Runtime topology

| Process | Default URL | Notes |
| --- | --- | --- |
| CRM API | `http://localhost:9040` | Controllers require authorization |
| Auth API | `http://localhost:9041` | Login, tenant/user administration and synchronization |
| Web | `http://localhost:9050` | Calls Auth and CRM services through typed/named clients |

Both APIs keep automatic migrations disabled by default. Apply migrations during deployment, or explicitly enable startup migration with `Database__ApplyMigrationsOnStartup=true`. Auth seeds a default service instance, tenant, and administrator only when initialization is enabled and secure seed configuration is present.

## Feature path

A typical CRM CRUD feature flows through:

`Domain aggregate -> Application command/query + handler -> Domain repository interface -> Infrastructure repository/DbContext -> API contract/controller -> Web API client -> Razor page/dialog`

Use existing Account, Contact, Ticket, or Opportunity implementations as templates. For Auth features, use AuthUser and Tenant flows.

## Integration flow

CRM requests a user synchronization operation, MassTransit persists it through the EF outbox, and the CRM saga coordinates the request. Auth consumes the shared contract, applies the operation to its owned user model, then publishes success or failure. CRM consumers update local state/projection. Contracts live in `SharedKernel/IntegrationEvents`; broker details stay outside Domain and Application.
