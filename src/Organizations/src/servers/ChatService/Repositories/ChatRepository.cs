using MongoDB.Driver;

using Pocco.Svc.Chats.Models;

namespace Pocco.Svc.Chats.Ripositories;

public class ChatRepository : IChatRepository {
  private readonly IMongoClient _client;

  public ChatRepository(IMongoClient client) {
    _client = client;
  }

  private IMongoCollection<Chat> GetCollection(string org_id) {
    var db = _client.GetDatabase(org_id);
    return db.GetCollection<Chat>("Chats");
  }

  public async Task<Chat> CreateAsync(string org_id, Chat chat) {
    var collection = GetCollection(org_id);
    await collection.InsertOneAsync(chat);
    return chat;
  }

  public async Task<Chat?> GetByIdAsync(string org_id, string id) {
    var collection = GetCollection(org_id);
    var filter = Builders<Chat>.Filter.Eq("_id", id);
     return await collection.Find(filter).FirstOrDefaultAsync();

  }

  public async Task<Chat?> UpdateAsync(string org_id, Chat updatechat) {
    var collection = GetCollection(org_id);
    var filter = Builders<Chat>.Filter.Eq("_id", updatechat);

    var updateDataBuilder = Builders<Chat>.Update;
    var updates = new List<UpdateDefinition<Chat>>();

    if (!string.IsNullOrWhiteSpace(updatechat.Name))
      updates.Add(updateDataBuilder.Set(c => c.Name, updatechat.Name));

     if (!string.IsNullOrWhiteSpace(updatechat.Description))
      updates.Add(updateDataBuilder.Set(c => c.Description, updatechat.Description));

      updates.Add(updateDataBuilder.Set(c => c.Is_Private, updatechat.Is_Private));

    if (updates.Count == 0) {
      return null;
      }

    var update = updateDataBuilder.Combine(updates);
    var result = await collection.UpdateOneAsync(filter, update);
    if (result.ModifiedCount > 0) {
      return null;
    }
    return await collection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<bool> DeleteAsync(string org_id, string id) {
    var collection = GetCollection(org_id);
    var filter = Builders<Chat>.Filter.Eq("_id", id);
    var result = await collection.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
