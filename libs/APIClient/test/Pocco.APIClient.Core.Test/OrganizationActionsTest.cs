using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;
using Xunit.Abstractions;

namespace Pocco.APIClient.Core.Test;

public class OrganizationActionsTest {

    private readonly ITestOutputHelper _output;
    private readonly ILogger<APIClient> _logger;

    private readonly APIClient _client;

    public OrganizationActionsTest(ITestOutputHelper output) {
        _output = output;
        _logger = output.BuildLoggerFor<APIClient>();

        var config = new APIClientConfigurations(APIClientType.User);
        _client = new APIClient(config, _logger);
    }

    [Fact]
    public async Task CreateOrganizationTestAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _output.WriteLine("Login failed. Cannot proceed with organization creation test.");
            Assert.Fail("Login failed. Cannot proceed with organization creation test.");
            return;
        }

        var randomSuffix = Guid.NewGuid().ToString()[..8];
        var createResult = await _client.CreateOrganizationAsync(new V0CreateOrganizationRequest {
            Base = new V0CreateXRequest {
                Name = "Test Organization " + randomSuffix
            },
            Description = "This is a test organization."
        });
        Assert.True(createResult.EventId != string.Empty, "Organization creation should return a valid EventId.");
        _output.WriteLine($"Organization created with Event ID: {createResult.EventId}");

        var logoutResult = await _client.SessionManager.LogoutAsync();
    }

    [Fact]
    public async Task UpdateOrganizationNameTestAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _output.WriteLine("Login failed. Cannot proceed with organization update test.");
            Assert.Fail("Login failed. Cannot proceed with organization update test.");
            return;
        }

        // Assuming an existing organization ID for testing
        var organizationId = Environment.GetEnvironmentVariable("POCCO_TEST_ORGANIZATION_ID") ?? string.Empty;
        var newName = "Updated Test Organization";

        var updateResult = await _client.UpdateOrganizationNameAsync(new V0UpdateOrganizationRequest {
            OrganizationId = organizationId,
            Name = newName
        });
        Assert.True(updateResult.EventId != string.Empty, "Organization name update should return a valid EventId.");
        _output.WriteLine($"Organization name updated. Event ID: {updateResult.EventId}");

        var logoutResult = await _client.SessionManager.LogoutAsync();
    }

    [Fact]
    public async Task GetOrganizationTestAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;

        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _output.WriteLine("Login failed. Cannot proceed with organization retrieval test.");
            Assert.Fail("Login failed. Cannot proceed with organization retrieval test.");
            return;
        }

        // Assuming an existing organization ID for testing
        var organizationId = Environment.GetEnvironmentVariable("POCCO_TEST_ORGANIZATION_ID") ?? string.Empty;

        var getResult = await _client.GetOrganizationAsync(new V0BaseRequest {
            Id = organizationId
        });
        Assert.True(getResult.OrganizationId == organizationId, "Retrieved organization ID should match the requested organization ID.");
        var resultJson = JsonSerializer.Serialize(getResult, new JsonSerializerOptions { WriteIndented = true });
        _output.WriteLine($"Organization retrieved: {resultJson}");

        var logoutResult = await _client.SessionManager.LogoutAsync();
    }

    [Fact]
    public async Task DeleteOrganizationTestAsync() {
        var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? string.Empty;
        var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? string.Empty;
        var loginResult = await _client.SessionManager.LoginAsync(email, password);

        if (!loginResult) {
            _output.WriteLine("Login failed. Cannot proceed with organization deletion test.");
            Assert.Fail("Login failed. Cannot proceed with organization deletion test.");
            return;
        }

        // Assuming an existing organization ID for testing
        var organizationId = Environment.GetEnvironmentVariable("POCCO_TEST_ORGANIZATION_ID") ?? string.Empty;
        var deleteResult = await _client.DeleteOrganizationAsync(new V0BaseRequest {
            Id = organizationId
        });
        Assert.True(deleteResult.EventId != string.Empty, "Organization deletion should return a valid EventId.");
        _output.WriteLine($"Organization deleted. Event ID: {deleteResult.EventId}");

        var logoutResult = await _client.SessionManager.LogoutAsync();
    }
}
