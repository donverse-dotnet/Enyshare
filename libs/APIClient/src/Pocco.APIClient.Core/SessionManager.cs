using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public class SessionManager {
    private SessionData? _sessionData;
    private readonly V0ApiService.V0ApiServiceClient _api;

    public SessionManager(V0ApiService.V0ApiServiceClient apiClient) {
        _api = apiClient;
    }

    /// <summary>
    /// アカウントへのログインメソッドを提供します。
    /// </summary>
    /// <param name="email">ログインしたいアカウントのメールアドレスです。</param>
    /// <param name="password">ログインしたいアカウントのパスワードです。この関数を呼び出す前にSHA256でHASH化しておく必要があります。</param>
    /// <returns>成功したかどうかを真偽値で返します。</returns>
    public async Task<bool> LoginASync(string email, string password) {
        try {
            var reply = await _api.AuthenticateAsync(new V0AccountAuthenticateRequest {
                Email = email,
                Password = password
            });

            _sessionData = SessionData.FromProto(reply);

            return true;
        } catch (RpcException ex) {
            Console.WriteLine($"Login failed: {ex.Status.Detail}");
            return false;
        } catch (Exception ex) {
            Console.WriteLine($"An unexpected error occurred during login: {ex.Message}");
            return false;
        } finally {
            if (_sessionData is not null) {
                Console.WriteLine($"Login successful. Session ID: {_sessionData.SessionId}");
            } else {
                Console.WriteLine("Login failed. No session data received.");
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
