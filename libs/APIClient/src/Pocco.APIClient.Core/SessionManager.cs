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

    /// <summary>
    /// セッションの自動更新を行います。
    /// <list type="bullet">
    /// <item>セッションが存在しない場合、2秒待機して再試行します。</item>
    /// <item>セッションが期限切れの場合、5秒待機して再試行します。</item>
    /// <item>セッションが5分以内に期限切れになる場合、セッションの更新を試みます。</item>
    /// <item>その他の場合、2秒待機して再試行します。</item>
    /// <item>このクラスが破棄されるか、キャンセルトークンがキャンセルされるまで繰り返します。</item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public async Task AutoRefreshSessionAsync() {
        do {
            if (_sessionData is null) {
                _client.Logger.LogWarning("Cannot refresh session: No session data available.");

                await Task.Delay(TimeSpan.FromMilliseconds(2000));
                continue;
            }

            if (_sessionData.IsExpired()) {
                _client.Logger.LogWarning("Session has expired. Please log in again.");

                await Task.Delay(TimeSpan.FromMilliseconds(5000));
                continue;
            }

            if (!_sessionData.NeedsRefresh(TimeSpan.FromMinutes(5))) {
                await Task.Delay(TimeSpan.FromMilliseconds(2000));
                continue;
            }

            _client.Logger.LogInformation("Refreshing session...");

            try {
                var header = _sessionData.ToMetadata();
                _client.Logger.LogInformation("{SessionData}", JsonSerializer.Serialize(_sessionData));
                var reply = await _client.API.VerifyTokenAsync(new Empty(), header);

                _sessionData = SessionData.FromProto(reply);

                _client.Logger.LogInformation("Session refreshed successfully. New Session ID: {SessionId}", _sessionData.SessionId);
            } catch (RpcException ex) {
                _client.Logger.LogWarning("RPC Error during session refresh: {ex}", ex);
            } catch (Exception ex) {
                _client.Logger.LogWarning("Unexpected error during session refresh: {ex}", ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(2000));
        } while (!_cancellationToken.IsCancellationRequested);
    }

    public void Dispose() {
        _client.Logger.LogInformation("Disposing SessionManager.");

        GC.SuppressFinalize(this);
    }
}
