using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pocco.APIClient.Core.Test;

public class EventDispatchTest {

    private readonly ITestOutputHelper _output;
    private readonly ILogger<APIClient> _logger;

    private readonly APIClient _client;

    public EventDispatchTest(ITestOutputHelper output) {
        _output = output;
        _logger = output.BuildLoggerFor<APIClient>();

        var config = new APIClientConfigurations(APIClientType.User);
        _client = new APIClient(config, _logger);
    }

    [Fact]
    public async Task TestLoginEventDispatchAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        Assert.True(loginResult, "Login should succeed with valid credentials.");
    }

    [Fact]
    public async Task TestLogoutEventDispatchAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;
        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _logger.LogError("Login failed. Cannot test logout event dispatch.");
            Assert.Fail("Login should succeed with valid credentials.");
            return;
        }

        var logoutResult = await _client.SessionManager.LogoutAsync();
        Assert.True(logoutResult, "Logout should succeed after a successful login.");
    }

    [Fact]
    public async Task TestSessionRefreshedEventDispatchAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;
        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _logger.LogError("Login failed. Cannot test session expiration event dispatch.");
            Assert.Fail("Login should succeed with valid credentials.");
            return;
        }

        try {
            await _client.SessionManager.AutoRefreshSessionAsync();
        } catch (Exception) {
            Assert.Fail("Auto-refresh session should not throw an exception.");
        }

        var logoutResult = await _client.SessionManager.LogoutAsync();
        Assert.True(logoutResult, "Logout should succeed after auto-refreshing session.");
    }
}
