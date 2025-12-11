using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// メンバーを取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">メンバーの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>１つのメンバーを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<Member> GetOrganizationMemberAsync(
        V0GetXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot get member: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.GetMemberAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }

    /// <summary>
    /// メンバーの一覧を取得するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">メンバーの取得に必要なデータ</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>複数のメンバーを返却します。</returns>
    /// <exception cref="InvalidOperationException">ログインできておらず、セッションデータがないときに投げられます。</exception>
    public async Task<V0ListMembersResponse> ListOrganizationMembersAsync(
        V0ListXRequest request,
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot getlist member: No session data available.");
        var header = sessionData.ToMetadata();

        var reply = await API.ListMembersAsync(request, header, null, cancellationToken); //TODO: わかりやすい名前に変更する
        return reply;
    }
}
