using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace NameCheckService
{
  // MongoDBのドキュメントモデル
  public class User
  {
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
  }

  // 名前重複チェックサービス
  public class NameChecker
  {
    private readonly IMongoCollection<User> _userCollection;

    public NameChecker(string connectionString, string dbName)
    {
      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(dbName);
      _userCollection = database.GetCollection<User>("users");
    }

    public async Task<bool> CheckNameOrThrowAsync(string name)
    {
      var filter = Builders<User>.Filter.Add(
        Builders<User>.Filter.Eq(u => u.Name, name),
        Builders<User>.Filter.Eq(u => u.DeleteAt, null)
      );

      var count = await _userCollection.CountDocumentsAsync(filter);

      if (count > 0)
      {
        throw new RpcException(
          new Status(StatusCode.Already.Exists, $"Name '{name}' is already in use.")
        );
      }
    }
  }
    // 実行例
  class Program
  {
    static async Task Main(string[] args)
    {
      var checker = new NameChecker("mongodb://localhost:27017", "your_database_name");

      Console.Write("名前を入力してください： ");
      var name = Console.ReadLine()?.Trim();

      try
      {
        await checker.CheckNameOrThrowAsync(name);
        Console.WriteLine("使用可能です");
      }
      catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
      {
        Console.WriteLine($"重複エラー: {ex.Status.Detail}");
      }
    }
  }
}
