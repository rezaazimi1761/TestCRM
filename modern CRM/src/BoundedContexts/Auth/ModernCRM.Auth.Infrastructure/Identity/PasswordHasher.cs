using ModernCRM.Auth.Application.Handlers;

namespace ModernCRM.Auth.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    public bool Verify(string password, string hash) => Hash(password) == hash;
}
