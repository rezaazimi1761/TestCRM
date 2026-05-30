using AuthService.Protos;
using Grpc.Net.Client;

namespace TestCRM.Infrastructure.GrpcClients;

/// <summary>
/// Thin wrapper around the generated gRPC client so it can be injected as a scoped service.
/// </summary>
public interface IAuthGrpcClient
{
    Task<ValidateTokenResponse> ValidateTokenAsync(string token, CancellationToken ct = default);
    Task<UserResponse>          GetUserByIdAsync  (int id,      CancellationToken ct = default);
    Task<UserClaimsResponse>    GetUserClaimsAsync(int userId,  CancellationToken ct = default);
}

public class AuthGrpcClient : IAuthGrpcClient, IDisposable
{
    private readonly GrpcChannel            _channel;
    private readonly AuthGrpc.AuthGrpcClient _client;

    public AuthGrpcClient(IConfiguration cfg)
    {
        var address = cfg["AuthService:GrpcUrl"]
                      ?? throw new InvalidOperationException("AuthService:GrpcUrl is not configured");

        _channel = GrpcChannel.ForAddress(address);
        _client  = new AuthGrpc.AuthGrpcClient(_channel);
    }

    public async Task<ValidateTokenResponse> ValidateTokenAsync(string token, CancellationToken ct)
        => await _client.ValidateTokenAsync(new ValidateTokenRequest { Token = token }, cancellationToken: ct);

    public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken ct)
        => await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = id }, cancellationToken: ct);

    public async Task<UserClaimsResponse> GetUserClaimsAsync(int userId, CancellationToken ct)
        => await _client.GetUserClaimsAsync(new GetUserClaimsRequest { UserId = userId }, cancellationToken: ct);

    public void Dispose() => _channel.Dispose();
}
