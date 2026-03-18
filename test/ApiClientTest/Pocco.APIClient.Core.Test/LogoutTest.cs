using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pocco.APIClient.Core.Test;

public class LogoutTest {

    private readonly ITestOutputHelper _output;
    private readonly ILogger<APIClient> _logger;

    private readonly APIClient _client;

    public LogoutTest(ITestOutputHelper output) {
        _output = output;
        _logger = output.BuildLoggerFor<APIClient>();

        var config = new APIClientConfigurations(APIClientType.User);
        _client = new APIClient(config, _logger);
    }

    [Fact]
    public async Task TestLogoutAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _output.WriteLine("Login failed. Cannot proceed with logout test.");
            Assert.Fail("Login failed. Cannot proceed with logout test.");
            return;
        }

        var logoutResult = await _client.SessionManager.LogoutAsync();
        Assert.True(logoutResult, "Logout should succeed after a successful login.");
    }
}
