using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// アカウント情報を更新するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">アカウントの登録情報の更新をするためのデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>成功すれば<seealso cref="V0BaseAccount"/>、失敗すれば例外</returns>
    /// <exception cref="InvalidOperationException">セッションデータが無ければ処理を中断します。</exception>
    public async Task<V0BaseAccount> UpdateAccountAsync(
        V0AccountUpdateProfileRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot update account: No session data available.");
        var headers = sessionData.ToMetadata();

        var response = await API.UpdateProfileAsync(
            request,
            headers: headers,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
