using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    // TODO: Add logging

    private readonly APIClientConfigurations _config;
    private readonly V0ApiService.V0ApiServiceClient _api;
    private readonly SessionManager _sessionManager;

    /// <summary>
    /// APIClientを<seealso cref="APIClientConfigurations"/>と共に初期化します。
    /// </summary>
    /// <param name="config">クライアントの設定</param>
    public APIClient(APIClientConfigurations config) {
        _config = config;
        _config.Logger.LogInformation("Initializing APIClient with endpoint: {Endpoint}", config.APIEndpoint);

        var channel = GrpcChannel.ForAddress(_config.APIEndpoint);
        _api = new V0ApiService.V0ApiServiceClient(channel);

        _sessionManager = new SessionManager(_api);

        _config.Logger.LogInformation("APIClient initialized successfully.");
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
