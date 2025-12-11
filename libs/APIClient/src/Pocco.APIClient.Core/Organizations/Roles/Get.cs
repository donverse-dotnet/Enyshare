using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// ロールを取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">ロールの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>１つのロールを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<Role> GetOrganizationRoleAsync(
        V0GetXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot get role: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.GetRoleAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }

    /// <summary>
    /// ロールの一覧を取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">ロールの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>複数のロールを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<V0ListRolesResponse> ListOrganizationRolesAsync(
        V0ListXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot getlist role: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.ListRolesAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
