using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;
using Xunit.Abstractions;

namespace Pocco.APIClient.Core.Test;

public class AccountActionsTest {

    private readonly ITestOutputHelper _output;
    private readonly ILogger<APIClient> _logger;

    private readonly APIClient _client;

    private readonly string _testEmail;
    private readonly string _testPassword;

    public AccountActionsTest(ITestOutputHelper output) {
        _output = output;
        _logger = output.BuildLoggerFor<APIClient>();

        _testEmail = Guid.NewGuid().ToString("N").Substring(0, 8) + "@example.com";
        _testPassword = "TestPassword" + Guid.NewGuid().ToString("N").Substring(0, 8);

        var config = new APIClientConfigurations(APIClientType.User);
        _client = new APIClient(config, _logger);
    }

    [Fact]
    public async Task CreateAccountTestAsync() {
        var createReply = await _client.CreateAccountAsync(
            new V0AccountRegisterRequest {
                Email = _testEmail,
                Password = _testPassword
            }
        );

        Assert.IsType<Empty>(createReply);
    }

    [Fact]
    public async Task GetAccountTestAsync() {
        await CreateAccountTestAsync();

        var loginResult = await _client.SessionManager.LoginAsync(_testEmail, _testPassword);
        Assert.True(loginResult, "Login should succeed with valid credentials.");

        var accountInfo = await _client.GetAccountAsync(
            new V0AccountGetProfileRequest {
                UserId = _client.SessionManager.GetSessionData()!.AccountId
            }
        );

        Assert.Equal(_testEmail.Split('@').First(), accountInfo.Username);
    }

    [Fact]
    public async Task UpdateAccountTestAsync() {
        await CreateAccountTestAsync();

        var loginResult = await _client.SessionManager.LoginAsync(_testEmail, _testPassword);
        Assert.True(loginResult, "Login should succeed with valid credentials.");

        var newUsername = "User_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var updateReply = await _client.UpdateAccountAsync(
            new V0AccountUpdateProfileRequest {
                UserId = _client.SessionManager.GetSessionData()!.AccountId,
                Username = newUsername
            }
        );

        Assert.IsType<V0BaseAccount>(updateReply);

        var accountInfo = await _client.GetAccountAsync(
            new V0AccountGetProfileRequest {
                UserId = _client.SessionManager.GetSessionData()!.AccountId
            }
        );

        Assert.Equal(newUsername, accountInfo.Username);
    }

    [Fact]
    public async Task DeleteAccountTestAsync() {
        await CreateAccountTestAsync();

        var loginResult = await _client.SessionManager.LoginAsync(_testEmail, _testPassword);
        Assert.True(loginResult, "Login should succeed with valid credentials.");

        var deleteReply = await _client.DeleteAccountAsync();

        Assert.IsType<Empty>(deleteReply);
    }
}
