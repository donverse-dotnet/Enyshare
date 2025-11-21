using Google.Protobuf.WellKnownTypes;

namespace Pocco.APIClient.Core;

public partial class APIClient {
    public async Task<Empty> DeleteAccountAsync(
        CancellationToken cancellationToken = default
    ) {
        var sessionData = SessionManager.GetSessionData() ?? throw new InvalidOperationException("Cannot delete account: No session data available.");
        var headers = sessionData.ToMetadata();

        var response = await API.DeleteAccountAsync(
            new Empty(),
            headers: headers,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
