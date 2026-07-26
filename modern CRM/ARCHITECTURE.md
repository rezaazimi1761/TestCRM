# ModernCRM Tactical DDD Architecture

این فولدر یک بازنویسی مفهومی و buildable از TestCRM بر اساس Tactical Design DDD است.

## Layers per Bounded Context

- Domain: Entities, ValueObjects, Aggregates, Domain Events, Repository Interfaces, Specifications
- Application: Commands, Queries, DTOs, Handlers
- Infrastructure: Persistence, Repository Implementations, Messaging, Identity
- Presentation: API Controllers

## Bounded Contexts

### Auth

Auth مالک aggregate اصلی `AuthUser` است. Credential، Role، Claim، Tenant و Saga وضعیت integration در این context قرار دارند.

### CRM

CRM مالک aggregateهای `Account`, `Contact`, `Ticket`, `Opportunity` است. مدل user در CRM فقط `CrmUserReference` است و با eventهای Auth sync می‌شود.

## Integration

زبان منتشرشده بین contextها در `SharedKernel/IntegrationEvents` قرار دارد:

- `UserIntegrationEvent`
- `UserIntegrationAppliedEvent`
- `UserIntegrationOperation`

CRM consumer نقش ACL دارد و مدل Auth را به projection داخلی CRM ترجمه می‌کند.
