using Google.Protobuf.WellKnownTypes;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    /// <summary>
    /// アカウントを新規作成するためのメソッドを提供します。
    /// </summary>
    /// <param name="request">アカウントのメールアドレスとパスワード</param>
    /// <param name="cancellationToken">このメソッドをキャンセルするためのトークン</param>
    /// <returns>成功すれば<seealso cref="Empty"/>、失敗すれば例外</returns>
    /// <exception cref="InvalidOperationException">ログインしている状態での作成を否定する場合に発火される。</exception>
    public async Task<Empty> CreateAccountAsync(
        V0AccountRegisterRequest request,
        CancellationToken cancellationToken = default
    ) {
        if (SessionManager.GetSessionData() is not null) {
            throw new InvalidOperationException("Cannot create account while logged in.");
        }

        var response = await API.RegisterAsync(
            request,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
