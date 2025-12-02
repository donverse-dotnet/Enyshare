using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// ロールを削除するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">ロールの削除に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>イベントIDを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<V0EventInvokedResponse> DeleteOrganizationRoleAsync(
        V0BaseRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot delete role: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.DeleteRoleAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
