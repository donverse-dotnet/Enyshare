using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// メッセージを取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">メッセージの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>１つのメッセージを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<Message> GetOrganizationMessageAsync(
        V0BaseRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot get message: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.GetMessageAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }

    public async Task<V0ListMessagesResponse> ListOrganizationMessageAsync(
        V0ListMessagesRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot getlist message: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.ListMessagesAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
