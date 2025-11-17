using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// 組織を新規作成するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">組織の作成に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>イベントIDを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<V0EventInvokedResponse> CreateOrganizationAsync(
        V0CreateXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot create organization: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.CreateAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
