using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// メッセージを削除するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">メッセージの削除に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>イベントIDを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<V0EventInvokedResponse> DeleteOrganizationMessageAsync(
        V0DeleteMessageRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot delete message: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.DeleteMessageAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
