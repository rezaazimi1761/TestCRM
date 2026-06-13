using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Consumers;

/// <summary>
/// Single CRM projection consumer for all AuthService user operations.
/// Operation is carried by UserIntegrationEvent.Operation instead of using separate consumers.
/// </summary>
public class UserIntegrationConsumer : IConsumer<UserIntegrationEvent>
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserIntegrationConsumer> _logger;

    public UserIntegrationConsumer(AppDbContext db, ILogger<UserIntegrationConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserIntegrationEvent> context)
    {
        var evt = context.Message;
        var ct = context.CancellationToken;

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.TenantId == evt.TenantId &&
                (u.AuthUserId == evt.AuthUserId || u.Username == evt.Username), ct);

        switch (evt.Operation)
        {
            case UserIntegrationOperation.Created:
            case UserIntegrationOperation.Updated:
                if (user is null)
                {
                    _db.Users.Add(new AppUser
                    {
                        AuthUserId = evt.AuthUserId,
                        TenantId = evt.TenantId,
                        Username = evt.Username,
                        Email = evt.Email,
                        FirstName = evt.FirstName,
                        LastName = evt.LastName,
                        Role = evt.Role,
                        IsActive = evt.IsActive
                    });
                }
                else
                {
                    user.AuthUserId = evt.AuthUserId;
                    user.Username = evt.Username;
                    user.Email = evt.Email;
                    user.FirstName = evt.FirstName;
                    user.LastName = evt.LastName;
                    user.Role = evt.Role;
                    user.IsActive = evt.IsActive;
                    user.IsDeleted = false;
                }
                break;

            case UserIntegrationOperation.Deleted:
                if (user is not null)
                {
                    user.AuthUserId = evt.AuthUserId;
                    user.IsActive = false;
                    user.IsDeleted = true;
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(evt.Operation), evt.Operation, "Unknown user integration operation.");
        }
        await _db.SaveChangesAsync(ct);

        await context.Publish(new UserIntegrationAppliedEvent(
            evt.CorrelationId,
            evt.Operation,
            evt.AuthUserId,
            evt.TenantId,
            DateTime.UtcNow), ct);

        _logger.LogInformation(
            "Applied user integration event. operation={Operation} tenant={TenantId} authUserId={AuthUserId}",
            evt.Operation, evt.TenantId, evt.AuthUserId);
    }
}
