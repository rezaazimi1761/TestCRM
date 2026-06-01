using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestCRM.Infrastructure.ServiceInstance;

/// <summary>
/// On first startup:
///  1. Ensure <c>ServiceInstance:Id</c> exists in appsettings.json — generate a GUID if missing.
///  2. Register this instance with the Auth service so it can be assigned to tenants.
///     If AuthService is not yet reachable, keeps retrying in the background until it succeeds.
/// </summary>
public class ServiceInstanceBootstrapper : IHostedService, IDisposable
{
    private readonly IConfiguration                       _cfg;
    private readonly IWebHostEnvironment                  _env;
    private readonly IHttpClientFactory                   _http;
    private readonly ILogger<ServiceInstanceBootstrapper> _log;

    private CancellationTokenSource? _cts;
    private Task?                    _retryTask;

    public ServiceInstanceBootstrapper(
        IConfiguration cfg,
        IWebHostEnvironment env,
        IHttpClientFactory http,
        ILogger<ServiceInstanceBootstrapper> log)
    {
        _cfg  = cfg;
        _env  = env;
        _http = http;
        _log  = log;
    }

    public Task StartAsync(CancellationToken ct)
    {
        var settingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");

        // 1. Load + parse appsettings.json (synchronously — we're in StartAsync)
        var root = JsonNode.Parse(File.ReadAllText(settingsPath))!;
        var instanceNode = (root["ServiceInstance"] ??= new JsonObject()).AsObject();

        var idStr = instanceNode["Id"]?.GetValue<string>();
        Guid id;
        var generatedNow = false;
        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out id))
        {
            id = Guid.NewGuid();
            instanceNode["Id"] = id.ToString();
            generatedNow = true;
        }

        var name = instanceNode["Name"]?.GetValue<string>() ?? Environment.MachineName + "-crm";
        instanceNode["Name"] ??= name;

        var apiUrl = instanceNode["ApiUrl"]?.GetValue<string>()
                  ?? _cfg["ServiceInstance:PublicUrl"]
                  ?? "http://localhost:9040";
        instanceNode["ApiUrl"] ??= apiUrl;

        if (generatedNow)
        {
            File.WriteAllText(settingsPath,
                root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            _log.LogInformation("Generated new ServiceInstance.Id {Id} — persisted to appsettings.json", id);
        }
        else
        {
            _log.LogInformation("Using existing ServiceInstance.Id {Id}", id);
        }

        // 2. Kick off background registration with retry — don't block app startup.
        _cts       = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _retryTask = Task.Run(() => RegisterWithRetryAsync(id, name, apiUrl, _cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private async Task RegisterWithRetryAsync(Guid id, string name, string apiUrl, CancellationToken ct)
    {
        var authUrl = (_cfg["AuthService:Url"] ?? "http://localhost:9041").TrimEnd('/');
        var attempt = 0;

        while (!ct.IsCancellationRequested)
        {
            attempt++;
            try
            {
                var http = _http.CreateClient();
                http.BaseAddress = new Uri(authUrl);
                http.Timeout     = TimeSpan.FromSeconds(5);

                var res = await http.PostAsJsonAsync("/api/service-instances/register", new
                {
                    Id          = id,
                    Name        = name,
                    ApiUrl      = apiUrl,
                    Description = (string?)null
                }, ct);

                if (res.IsSuccessStatusCode)
                {
                    _log.LogInformation(
                        "Registered with AuthService at {Url} on attempt {Attempt}", authUrl, attempt);
                    return;
                }

                _log.LogWarning(
                    "AuthService registration returned {Status} (attempt {Attempt}) — will retry…",
                    res.StatusCode, attempt);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogDebug(
                    "AuthService not reachable at {Url} (attempt {Attempt}): {Msg}",
                    authUrl, attempt, ex.Message);
            }

            // Back off: 2s for the first minute, then 15s
            var delay = attempt < 30 ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(15);
            try { await Task.Delay(delay, ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _cts?.Cancel();
        if (_retryTask is not null)
        {
            try { await _retryTask; } catch { /* swallow */ }
        }
    }

    public void Dispose() => _cts?.Dispose();
}
