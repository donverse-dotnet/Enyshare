using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pocco.APIClient.Core.Test;

public class LoginTest {

    private readonly ITestOutputHelper _output;
    private readonly ILogger<APIClient> _logger;

    private readonly APIClient _client;

    public LoginTest(ITestOutputHelper output) {
        _output = output;
        _logger = output.BuildLoggerFor<APIClient>();

        var config = new APIClientConfigurations(APIClientType.User);
        _client = new APIClient(config, _logger);
    }

    [Fact]
    public async Task TestLoginAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);
        Assert.True(loginResult, "Login should succeed with valid credentials.");
    }
}
