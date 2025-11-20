using Microsoft.Extensions.Logging;
using Pocco.APIClient.Core;
using Pocco.Libs.Protobufs.Services;

var logger = LoggerFactory.Create<APIClient>();

var config = new APIClientConfigurations(APIClientType.User);
var client = new APIClient(config, logger);

var email = Environment.GetEnvironmentVariable("POCCO_TEST_EMAIL") ?? throw new Exception("POCCO_TEST_EMAIL is not set.");
var password = Environment.GetEnvironmentVariable("POCCO_TEST_PASSWORD") ?? throw new Exception("POCCO_TEST_PASSWORD is not set.");
await client.SessionManager.LoginAsync(email, password);
Console.WriteLine("Logged in successfully.");

var sessionData = client.SessionManager.GetSessionData() ?? throw new Exception("Session data is null after login.");
var listenRequest = new ListenRequest {
    UserId = sessionData.AccountId,
    SessionId = sessionData.SessionId,
};
listenRequest.Topics.Add(V0EventTopics.EventTopicUser);
listenRequest.Topics.Add(V0EventTopics.EventTopicOrganization);
await client.EventListener.StartListeningAsync(listenRequest);

await Task.Delay(2000); // 少し待ってからイベント購読を更新

var randomOrgName = $"TestOrg-{Guid.NewGuid().ToString().Substring(0, 8)}";
var orgCreateReply = await client.CreateOrganizationAsync(new V0CreateOrganizationRequest {
    Base = new V0CreateXRequest {
        Name = randomOrgName,
    },
    Description = "This is a test organization.",
});
Console.WriteLine($"Organization creation requested. Event ID: {orgCreateReply.EventId}");

await Task.Delay(5000); // イベント受信のために少し待機

var orgId = client.EventListener.TempOrganizationIds.LastOrDefault() ?? throw new Exception("Organization ID not found from events.");
var orgUpdareReply = await client.UpdateOrganizationNameAsync(new V0UpdateOrganizationNameRequest {
    OrganizationId = orgId,
    Name = randomOrgName + "-Updated",
});
Console.WriteLine($"Organization name update requested. Event ID: {orgUpdareReply.EventId}");

await Task.Delay(5000); // イベント受信のために少し待機

var orgDeleteReply = await client.DeleteOrganizationAsync(new V0BaseRequest {
    Id = orgId,
});
Console.WriteLine($"Organization deletion requested. Event ID: {orgDeleteReply.EventId}");

await Task.Delay(5000); // 最後に少し待ってから終了
client.Dispose();

#region Helper Classes
class Logger<T> : ILogger<T> {
    private readonly string _name;

    public Logger(string name) {
        _name = name;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter) {
        Console.WriteLine($"[{_name}] {logLevel}: {formatter(state, exception!)}");
    }
}
static class LoggerFactory {
    public static ILogger<T> Create<T>() where T : class {
        return new Logger<T>("AllFeaturedTest");
    }
}
#endregion
