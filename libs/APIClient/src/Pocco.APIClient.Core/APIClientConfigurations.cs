namespace Pocco.APIClient.Core;

public class APIClientConfigurations {
    public readonly string APIEndpoint;
    public readonly APIClientType ClientType = APIClientType.Bot;
    public static APIClientConfigurations Default { get; } = new APIClientConfigurations();

    public APIClientConfigurations() {
        APIEndpoint = Environment.GetEnvironmentVariable("POCCO_API_ENDPOINT") ?? string.Empty;
    }

    public APIClientConfigurations(APIClientType clientType) {
        APIEndpoint = Environment.GetEnvironmentVariable("POCCO_API_ENDPOINT") ?? string.Empty;
        ClientType = clientType;
    }
}
