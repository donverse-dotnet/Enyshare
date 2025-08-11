using MongoDB.Driver;

namespace MessageService.Services;

public class DatabaseManager : IDisposable {
  public readonly Dictionary<string, Database> OrganizationClients = [];

  public class Database(DateTime time, IMongoDatabase mongoDatabase) {
    public DateTime LastUsed { get; set; } = time;
    public IMongoDatabase MongoDatabase { get; set; } = mongoDatabase;

    public void UpdateLastUsed() {
      LastUsed = DateTime.UtcNow;
    }
  }

  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private CancellationToken CancellationToken => _cancellationTokenSource.Token;

  public DatabaseManager() {
    // Start a background task to clean up unused clients
    Task.Run(() => DisposeUnusedClientsAsync(CancellationToken));
  }

  public void Dispose() {
    _cancellationTokenSource.Cancel();
    OrganizationClients.Clear();

    GC.SuppressFinalize(this);
  }

  public IMongoDatabase GetDatabaseClient(string organizationId) {
    if (OrganizationClients.TryGetValue(organizationId, out var db)) {
      db.UpdateLastUsed();
      return db.MongoDatabase;
    }

    // Create a new MongoDB client for the organization
    var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
    var mongoClient = new MongoClient(connectionString);
    var mongoDatabase = mongoClient.GetDatabase($"org_{organizationId}");

    var newDb = new Database(DateTime.UtcNow, mongoDatabase);
    OrganizationClients[organizationId] = newDb;

    return mongoDatabase;
  }

  private async Task DisposeUnusedClientsAsync(CancellationToken cancellationToken = default) {
    while (cancellationToken.IsCancellationRequested is false) {
      var now = DateTime.UtcNow;
      var unusedClients = OrganizationClients
          .Where(kvp => (now - kvp.Value.LastUsed).TotalMinutes > 30) // 30 minutes threshold
          .Select(kvp => kvp.Key)
          .ToList();

      foreach (var clientId in unusedClients) {
        OrganizationClients.Remove(clientId);
      }

      await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken); // Check every 5 minutes
    }

    if (cancellationToken.IsCancellationRequested) {
      foreach (var client in OrganizationClients.Values) {
        client.MongoDatabase.Client.Dispose();
      }
    }
  }
}
