using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient : IDisposable {
    public readonly ILogger<APIClient> Logger;
    public readonly CancellationTokenSource CancellationTokenSource;
    public readonly V0ApiService.V0ApiServiceClient API;
    public readonly SessionManager SessionManager;
    public readonly EventHub EventHub;

    private readonly APIClientConfigurations _config;

    /// <summary>
    /// APIClientを<seealso cref="APIClientConfigurations"/>と共に初期化します。
    /// </summary>
    /// <param name="config">クライアントの設定</param>
    /// <param name="logger">ロガーインスタンス</param>
    public APIClient(APIClientConfigurations config, ILogger<APIClient> logger) {
        _config = config;
        Logger = logger;
        CancellationTokenSource = new CancellationTokenSource();
        Logger.LogInformation("Initializing APIClient...");

        var channel = GrpcChannel.ForAddress(_config.APIEndpoint);
        API = new V0ApiService.V0ApiServiceClient(channel);

        SessionManager = new SessionManager(this, CancellationTokenSource.Token);
        EventHub = new EventHub();

        EventHub.GetObservable<ClientEvents.OnLog>()
            .Subscribe(e => Logger.LogInformation("{Message}", e.Message));
        EventHub.GetObservable<ClientEvents.OnError>()
            .Subscribe(e => Logger.LogError("{Message}", e.Message));

        EventHub.GetObservable<ClientEvents.OnClientLoggedIn>()
            .Subscribe(e => Logger.LogInformation("Client logged in. SessionId: {SessionId}", e.Session.SessionId));
        EventHub.GetObservable<ClientEvents.OnClientLoggedOut>()
            .Subscribe(e => Logger.LogInformation("Client logged out."));
        EventHub.GetObservable<ClientEvents.OnSessionExpired>()
            .Subscribe(e => Logger.LogWarning("Session expired."));
        EventHub.GetObservable<ClientEvents.OnSessionRefreshed>()
            .Subscribe(e => Logger.LogInformation("Session refreshed. New SessionId: {SessionId}", e.Session.SessionId));

        Logger.LogInformation("APIClient initialized with {Endpoint}.", _config.APIEndpoint);
    }

    public void Dispose() {
        Logger.LogInformation("Disposing APIClient...");

        CancellationTokenSource.Cancel();
        SessionManager.Dispose();
        EventHub.Dispose();
        CancellationTokenSource.Dispose();

        Logger.LogInformation("APIClient disposed.");
        GC.SuppressFinalize(this);
    }
}
