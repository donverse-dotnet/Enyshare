using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    public readonly ILogger<APIClient> Logger;

    private readonly APIClientConfigurations _config;
    private readonly V0ApiService.V0ApiServiceClient _api;
    private readonly SessionManager _sessionManager;

    /// <summary>
    /// APIClientを<seealso cref="APIClientConfigurations"/>と共に初期化します。
    /// </summary>
    /// <param name="config">クライアントの設定</param>
    public APIClient(APIClientConfigurations config, ILogger<APIClient> logger) {
        _config = config;
        Logger = logger;
        Log("Initializing APIClient...", null);

        var channel = GrpcChannel.ForAddress(_config.APIEndpoint);
        _api = new V0ApiService.V0ApiServiceClient(channel);

        _sessionManager = new SessionManager(_api);

        Log("APIClient initialized with endpoint: {Endpoint}", null, LogLevel.Information, _config.APIEndpoint);
    }

    public async Task LogAsync(string? message, Exception? exception, LogLevel level = LogLevel.Information, params object?[] args) {
        Log(message, exception, level, args);
        await Task.CompletedTask;
    }
    public void Log(string? message, Exception? exception, LogLevel level = LogLevel.Information, params object?[] args) {
        Logger.Log(level, exception, message, args);
    }

    /// <summary>
    /// アカウントへのログインを行います。
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns>ログインできたかどうか</returns>
    public async Task<bool> LoginAsync(string email, string password) {
        return await _sessionManager.LoginASync(email, password);
    }
}
