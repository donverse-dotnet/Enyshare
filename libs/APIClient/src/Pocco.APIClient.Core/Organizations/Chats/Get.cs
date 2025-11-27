using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// チャットを取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">チャットの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>１つのチャットを返却します。</returns>
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

     public async Task<V0ListChatsResponse> ListOrganizationChatsAsync(
        V0ListXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot getlist chat: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.ListChatsAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
