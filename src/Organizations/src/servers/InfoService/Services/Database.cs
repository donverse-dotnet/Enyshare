using MongoDB.Driver;

public class MongoConnection
{
  public static void Connect()
  {
    var client = new MongoClient("mongodb://localhost:27017");

    var database = client.GetDatabase("mydatabase");
    var collection = database.GetCollection<User>("users");
  }
}
