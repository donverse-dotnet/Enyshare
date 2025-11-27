using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// 組織を取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">組織の取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>１つの組織を返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<Chat> GetOrganizationChatAsync(
        V0BaseRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot get chat: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.GetChatAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
