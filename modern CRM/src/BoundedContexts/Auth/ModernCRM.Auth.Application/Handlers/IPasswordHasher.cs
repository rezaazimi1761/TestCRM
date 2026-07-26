using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.DTO;
using ModernCRM.Auth.Application.Queries;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Application.Handlers;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
