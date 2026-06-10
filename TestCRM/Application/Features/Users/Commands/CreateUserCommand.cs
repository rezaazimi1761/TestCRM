using System.ComponentModel.DataAnnotations;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Application.Interfaces;
using Shared.Contracts.Events;
using TestCRM.Application.Common;
using TestCRM.Domain.Entities;
using TestCRM.Infrastructure.Persistence;

namespace TestCRM.Application.Features.Users.Commands;

public record CreateUserCommand(
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string Username,
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string FirstName,
    [Required(AllowEmptyStrings = false)] [StringLength(100)] string LastName,
    [Required(AllowEmptyStrings = false)] [EmailAddress] [StringLength(255)] string Email,
    [Required(AllowEmptyStrings = false)] [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be 6–128 characters.")] string Password,
    string Role = "User"
) : IRequest<int>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly AppDbContext      _db;
    private readonly IPublishEndpoint  _bus;
    private readonly ITenantService    _tenant;

    public CreateUserCommandHandler(AppDbContext db, IPublishEndpoint bus, ITenantService tenant)
    {
        _db     = db;
        _bus    = bus;
        _tenant = tenant;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email    = request.Email.Trim().ToLower();
        var username = request.Username.Trim().ToLower();

        if (await _db.Users.AnyAsync(u => u.Username.ToLower() == username, ct))
            throw new ValidationException($"Username '{username}' is already taken in this tenant.");

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email, ct))
            throw new DuplicateEmailException($"A user with email '{email}' already exists in this tenant.");

        // Hash once — same hash is carried in the event so AuthService
        // stores the identical hash (BCrypt.Verify works on any hash origin).
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new AppUser
        {
            Username     = username,
            FirstName    = request.FirstName.Trim(),
            LastName     = request.LastName.Trim(),
            Email        = email,
            PasswordHash = hash,
            Role         = request.Role
        };
        _db.Users.Add(user);

        // Publish via Outbox — stored in OutboxMessage table in the SAME
        // transaction as SaveChangesAsync below. The OutboxDeliveryService
        // will forward to RabbitMQ after the commit.
        var tenantId = _tenant.GetCurrentTenant();
        await _bus.Publish(new UserCreatedEvent(
            CorrelationId: NewId.NextGuid(),
            TenantId:      tenantId,   // read from ITenantService (not yet on entity)
            Username:      username,
            Email:         email,
            FirstName:     user.FirstName,
            LastName:      user.LastName,
            PasswordHash:  hash,
            Role:          user.Role
        ), ct);

        await _db.SaveChangesAsync(ct);
        return user.Id;
    }
}
