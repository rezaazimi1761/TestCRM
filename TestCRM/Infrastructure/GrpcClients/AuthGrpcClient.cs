using Shared.Protos;
using Grpc.Net.Client;

namespace TestCRM.Infrastructure.GrpcClients;

public interface IAuthGrpcClient
{
    Task<ValidateTokenResponse> ValidateTokenAsync  (string token,  CancellationToken ct = default);
    Task<UserResponse>          GetUserByIdAsync    (int id,        CancellationToken ct = default);
    Task<UserClaimsResponse>    GetUserClaimsAsync  (int userId,    CancellationToken ct = default);
    Task<TenantResponse>        GetTenantBySlugAsync(string slug,   CancellationToken ct = default);
}

public class AuthGrpcClient : IAuthGrpcClient, IDisposable
{
    private readonly GrpcChannel            _channel;
    private readonly AuthGrpc.AuthGrpcClient _client;

    public AuthGrpcClient(IConfiguration cfg)
    {
        var address = cfg["AuthService:GrpcUrl"]
                      ?? throw new InvalidOperationException("AuthService:GrpcUrl is not configured");

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        _channel = GrpcChannel.ForAddress(address);
        _client  = new AuthGrpc.AuthGrpcClient(_channel);
    }

    public async Task<ValidateTokenResponse> ValidateTokenAsync(string token, CancellationToken ct)
        => await _client.ValidateTokenAsync(new ValidateTokenRequest { Token = token }, cancellationToken: ct);

    public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken ct)
        => await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = id }, cancellationToken: ct);

    public async Task<UserClaimsResponse> GetUserClaimsAsync(int userId, CancellationToken ct)
        => await _client.GetUserClaimsAsync(new GetUserClaimsRequest { UserId = userId }, cancellationToken: ct);

    public async Task<TenantResponse> GetTenantBySlugAsync(string slug, CancellationToken ct)
        => await _client.GetTenantBySlugAsync(new GetTenantBySlugRequest { Slug = slug }, cancellationToken: ct);

    public void Dispose() => _channel.Dispose();
}
