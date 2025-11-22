using System.Reactive.Linq;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public class SessionManager : IDisposable {
    private const int EXPIRE_BEFORE_MINUTES = 5;
    private const int REFRESH_SESSION_THRESHOLD_SECONDS = 20;
    private SessionData? _sessionData;
    private readonly APIClient _client;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CancellationToken _cancellationToken;

    public SessionManager(APIClient apiClient) {
        _client = apiClient;
        _cancellationToken = _cancellationTokenSource.Token;

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
            }, cancellationToken: _cancellationToken);

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
                _client.EventHub.Push(new ClientEvents.OnClientLoggedIn(ClientEvents.PRIVATE_EVENT_ID, _sessionData));
            } else {
                _client.Logger.LogWarning("Login failed. No session data obtained.");
            }
        }
    }

    /// <summary>
    /// アカウントからのログアウトメソッドを提供します。
    /// </summary>
    /// <returns>ログアウトできたかどうかを真偽値で表します。</returns>
    public async Task<bool> LogoutAsync() {
        if (_sessionData is null) {
            _client.Logger.LogWarning("Cannot logout: No session data available.");
            return false;
        }

        try {
            var header = _sessionData.ToMetadata();
            await _client.API.UnauthenticateAsync(new Empty(), header, cancellationToken: _cancellationToken);

            _client.Logger.LogInformation("Logout successful. Session ID: {SessionId}", _sessionData.SessionId);
            return true;
        } catch (RpcException ex) {
            _client.Logger.LogWarning("RPC Error during logout: {ex}", ex);
            return false;
        } catch (Exception ex) {
            _client.Logger.LogWarning("Unexpected error during logout: {ex}", ex);
            return false;
        } finally {
            _sessionData = null;
            _client.EventHub.Push(new ClientEvents.OnClientLoggedOut(ClientEvents.PRIVATE_EVENT_ID));
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
    /// <item>セッションが期限切れの場合、セッションの有効期限切れイベントを発行して処理を終了します。</item>
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

                await Task.Delay(TimeSpan.FromSeconds(REFRESH_SESSION_THRESHOLD_SECONDS));
                continue;
            }

            if (_sessionData.IsExpired()) {
                _client.Logger.LogWarning("Session has expired. Please log in again.");
                _client.EventHub.Push(new ClientEvents.OnSessionExpired(ClientEvents.PRIVATE_EVENT_ID));
                break;
            }

            if (!_sessionData.NeedsRefresh(TimeSpan.FromMinutes(EXPIRE_BEFORE_MINUTES))) {
                await Task.Delay(TimeSpan.FromSeconds(REFRESH_SESSION_THRESHOLD_SECONDS));
                continue;
            }

            _client.Logger.LogInformation("Refreshing session...");

            try {
                var header = _sessionData.ToMetadata();
                var reply = await _client.API.VerifyTokenAsync(new Empty(), header);

                _sessionData = SessionData.FromProto(reply);

                _client.Logger.LogInformation("Session refreshed successfully. New Session ID: {SessionId}", _sessionData.SessionId);
            } catch (RpcException ex) {
                _client.Logger.LogWarning("RPC Error during session refresh: {ex}", ex);
            } catch (Exception ex) {
                _client.Logger.LogWarning("Unexpected error during session refresh: {ex}", ex);
            }

            _client.EventHub.Push(new ClientEvents.OnSessionRefreshed(ClientEvents.PRIVATE_EVENT_ID, _sessionData));

            await Task.Delay(TimeSpan.FromSeconds(REFRESH_SESSION_THRESHOLD_SECONDS));
        } while (!_cancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// セッションの検証を行います。
    /// </summary>
    /// <param name="sessionData">検証したいセッションデータ</param>
    /// <returns><seealso cref="Task"/></returns>
    public async Task VerifySessionAsync(SessionData sessionData) {
        try {
            var header = sessionData.ToMetadata();
            var reply = await _client.API.VerifyTokenAsync(new Empty(), header, cancellationToken: _cancellationToken);

            _sessionData = SessionData.FromProto(reply);

            _client.Logger.LogInformation("Session verified successfully. Session ID: {SessionId}", _sessionData.SessionId);
        } catch (RpcException ex) {
            _client.Logger.LogWarning("RPC Error during session verification: {ex}", ex);
        } catch (Exception ex) {
            _client.Logger.LogWarning("Unexpected error during session verification: {ex}", ex);
        }
    }

    public void Dispose() {
        _client.Logger.LogInformation("Disposing SessionManager.");

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _client.Logger.LogInformation("SessionManager disposed.");
        GC.SuppressFinalize(this);
    }
}
