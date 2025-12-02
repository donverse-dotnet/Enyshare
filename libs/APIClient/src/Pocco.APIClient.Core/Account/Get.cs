using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// アカウント情報を取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">アカウントの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>成功すれば<seealso cref="V0BaseAccount"/>、失敗すれば例外</returns>
    /// <exception cref="InvalidOperationException">セッションデータが無ければ処理を中断するために発火。</exception>
    public async Task<V0BaseAccount> GetAccountAsync(
        V0AccountGetProfileRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot get account: No session data available.");
        var headers = sessionData.ToMetadata();

        var response = await API.GetProfileAsync(
            request,
            headers: headers,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
