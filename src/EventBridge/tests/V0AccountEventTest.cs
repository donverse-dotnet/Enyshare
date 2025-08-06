using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Svc.EventBridge.Protobufs;
using Pocco.Svc.EventBridge.Protobufs.Types;
using Xunit.Abstractions;

namespace Pocco.Svc.EventBridgeTest;

public class V0AccountEventTest {
  private readonly ILogger<V0AccountEventTest> _logger;

  public V0AccountEventTest(ITestOutputHelper outputHelper) {
    var factory = LoggerFactory.Create(builder => {
      builder.AddProvider(new LoggerProvider(outputHelper));
      builder.SetMinimumLevel(LogLevel.Information);
    });

    _logger = factory.CreateLogger<V0AccountEventTest>();
    _logger.LogInformation("V0AccountEventTest initialized.");
  }

  [Fact]
  public async Task AccountCreatedEvent_ShouldPublishSuccessfully() {
    _logger.LogInformation("Starting AccountCreatedEvent test...");

    // Arrange
    using var channel = GrpcChannel.ForAddress("http://localhost:5218");
    var client = new V0AccountEvents.V0AccountEventsClient(channel);
    var request = new V0AccountCreatedEvent {
      Id = "test-account-id",
      Email = "test@y.com",
      Username = "testuser",
      Role = "user",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
    };

    var now = DateTime.UtcNow;
    _logger.LogInformation("Request created at: {now}", now);

    // Act
    var response = await client.V0PublishAccountCreatedAsync(request);

    _logger.LogInformation("Response received at: {now}", DateTime.UtcNow);
    _logger.LogInformation("Response: Success={Success}, EventId={EventId}, ErrorMessage={ErrorMessage}",
      response.Success, response.EventId, response.ErrorMessage);
    _logger.LogInformation("Elapsed time: {Elapsed} ms", (DateTime.UtcNow - now).TotalMilliseconds);

    // Assert
    Assert.True(response.Success);
    Assert.NotEmpty(response.EventId);
    Assert.Empty(response.ErrorMessage);
    _logger.LogInformation("AccountCreatedEvent test completed successfully.");
    _logger.LogInformation("Test completed.");
  }

  [Fact]
  public async Task AccountUpdatedEvent_ShouldPublishSuccessfully() {
    _logger.LogInformation("Starting AccountUpdatedEvent test...");

    // Arrange
    using var channel = GrpcChannel.ForAddress("http://localhost:5218");
    var client = new V0AccountEvents.V0AccountEventsClient(channel);
    var request = new V0AccountUpdatedEvent {
      AccountModel = new V0AccountModel {
        AccountId = "test-account-id",
        Email = "test@y.com",
        EmailVerified = true,
        Username = "testuser",
        AvatarUrl = "http://example.com/avatar.png",
        StatusMessage = "Hello, world!",
        Role = "user",
        IsActive = true,
        CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
        UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      }
    };

    var now = DateTime.UtcNow;
    _logger.LogInformation("Request created at: {now}", now);

    // Act
    var response = await client.V0PublishAccountUpdatedAsync(request);
    _logger.LogInformation("Response received at: {now}", DateTime.UtcNow);
    _logger.LogInformation("Response: Success={Success}, EventId={EventId}, ErrorMessage={ErrorMessage}",
      response.Success, response.EventId, response.ErrorMessage);
    _logger.LogInformation("Elapsed time: {Elapsed} ms", (DateTime.UtcNow - now).TotalMilliseconds);
    // Assert
    Assert.True(response.Success);
    Assert.NotEmpty(response.EventId);
    Assert.Empty(response.ErrorMessage);
    _logger.LogInformation("AccountUpdatedEvent test completed successfully.");
    _logger.LogInformation("Test completed.");
  }

  [Fact]
  public async Task AccountModerationEvent_ShouldPublishSuccessfully() {
    _logger.LogInformation("Starting AccountModerationEvent test...");

    // Arrange
    using var channel = GrpcChannel.ForAddress("http://localhost:5218");
    var client = new V0AccountEvents.V0AccountEventsClient(channel);
    var request = new V0AccountModeratedEvent {
      AccountId = "test-account-id",
      Action = "moderate",
      Reason = "Test moderation",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
    };

    var now = DateTime.UtcNow;
    _logger.LogInformation("Request created at: {now}", now);

    // Act
    var response = await client.V0PublishAccountModeratedAsync(request);

    _logger.LogInformation("Response received at: {now}", DateTime.UtcNow);
    _logger.LogInformation("Response: Success={Success}, EventId={EventId}, ErrorMessage={ErrorMessage}",
      response.Success, response.EventId, response.ErrorMessage);
    _logger.LogInformation("Elapsed time: {Elapsed} ms", (DateTime.UtcNow - now).TotalMilliseconds);

    // Assert
    Assert.True(response.Success);
    Assert.NotEmpty(response.EventId);
    Assert.Empty(response.ErrorMessage);
    _logger.LogInformation("AccountModerationEvent test completed successfully.");
    _logger.LogInformation("Test completed.");
  }

  [Fact]
  public async Task AccountDisabledEvent_ShouldPublishSuccessfully() {
    _logger.LogInformation("Starting AccountDeletedEvent test...");

    // Arrange
    using var channel = GrpcChannel.ForAddress("http://localhost:5218");
    var client = new V0AccountEvents.V0AccountEventsClient(channel);
    var request = new V0AccountDisabledEvent {
      Success = true,
      AccountId = "test-account-id",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
    };

    var now = DateTime.UtcNow;
    _logger.LogInformation("Request created at: {now}", now);

    // Act
    var response = await client.V0PublishAccountDisabledAsync(request);

    _logger.LogInformation("Response received at: {now}", DateTime.UtcNow);
    _logger.LogInformation("Response: Success={Success}, EventId={EventId}, ErrorMessage={ErrorMessage}",
      response.Success, response.EventId, response.ErrorMessage);
    _logger.LogInformation("Elapsed time: {Elapsed} ms", (DateTime.UtcNow - now).TotalMilliseconds);

    // Assert
    Assert.True(response.Success);
    Assert.NotEmpty(response.EventId);
    Assert.Empty(response.ErrorMessage);
    _logger.LogInformation("AccountDisabledEvent test completed successfully.");
    _logger.LogInformation("Test completed.");
  }
}
