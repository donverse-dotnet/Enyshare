using Microsoft.Extensions.Logging;

namespace Pocco.APIClient.Core;

public class APIClientConfigurations {
    public readonly ILogger<APIClient> Logger;
    public readonly string APIEndpoint;
    public readonly APIClientType ClientType = APIClientType.Bot;

    public APIClientConfigurations(ILogger<APIClient> logger) {
        APIEndpoint = Environment.GetEnvironmentVariable("POCCO_API_ENDPOINT") ?? string.Empty;
        Logger = logger;
    }

    public APIClientConfigurations(ILogger<APIClient> logger, APIClientType clientType) {
        APIEndpoint = Environment.GetEnvironmentVariable("POCCO_API_ENDPOINT") ?? string.Empty;
        Logger = logger;
        ClientType = clientType;
    }

    public static APIClientConfigurations Default(ILogger<APIClient> logger) {
        return new APIClientConfigurations(logger);
    }
}
