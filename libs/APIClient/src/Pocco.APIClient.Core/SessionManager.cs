using Grpc.Core;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public class SessionManager {
    private SessionData? _sessionData;
    private readonly APIClient _client;

    public SessionManager(APIClient apiClient) {
        _client = apiClient;

        _client.Logger.LogInformation("SessionManager initialized.");
    }

    /// <summary>
    /// アカウントへのログインメソッドを提供します。
    /// </summary>
    /// <param name="email">ログインしたいアカウントのメールアドレスです。</param>
    /// <param name="password">ログインしたいアカウントのパスワードです。この関数を呼び出す前にSHA256でHASH化しておく必要があります。</param>
    /// <returns>成功したかどうかを真偽値で返します。</returns>
    public async Task<bool> LoginAsync(string email, string password) {
        try {
            var reply = await _client.API.AuthenticateAsync(new V0AccountAuthenticateRequest {
                Email = email,
                Password = password
            });

            _sessionData = SessionData.FromProto(reply);

            return true;
        } catch (RpcException ex) {
            _client.Logger.LogWarning("RPC Error during login: {ex}", ex);
            return false;
        } catch (Exception ex) {
            _client.Logger.LogWarning("Unexpected error during login: {ex}", ex);
            return false;
        } finally {
            if (_sessionData is not null) {
                _client.Logger.LogInformation("Login successful. Session ID: {SessionId}", _sessionData.SessionId);
            } else {
                _client.Logger.LogWarning("Login failed. No session data obtained.");
            }
        }
    }

    /// <summary>
    /// 現在のセッションデータを取得します。
    /// </summary>
    /// <returns>現在のセッションデータ。なければnullです。</returns>
    public SessionData? GetSessionData() {
        return _sessionData;
    }
}
