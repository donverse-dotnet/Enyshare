using MongoDB.Driver;
using MongoDB.Bson;

using Pocco.Svc.Chats.Models;

namespace Pocco.Svc.Chats.Ripositories;

public class ChatRepository : IChatRepository {
  private readonly IMongoClient _client;

  public ChatRepository(IMongoClient client) {
    _client = client;
  }

  private IMongoCollection<Chat> GetChatCollection(string org_id) {
    var db = _client.GetDatabase(org_id);
    return db.GetCollection<Chat>("Chats");
  }

  public async Task<Chat> CreateAsync(string org_id, Chat chat) {
    var chats = GetChatCollection(org_id);
    await chats.InsertOneAsync(chat);
    return chat;
  }

  public async Task<Chat> GetByIdAsync(string org_id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }
    var chats = GetChatCollection(org_id);
    var filter = Builders<Chat>.Filter.And(Builders<Chat>.Filter.Eq(c => c.Id, objectId.ToString())
    );

    return await chats.Find(filter).FirstOrDefaultAsync();

  }

  public async Task<bool> TryUpdateAsync(string orgId, string chatId, Chat updatechat) {
    var latestChat = await GetByIdAsync(orgId, chatId);
    if (!ObjectId.TryParse(chatId, out var objectId) || !ObjectId.TryParse(orgId, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var chats = GetChatCollection(orgId);

    var filter = Builders<Chat>.Filter.And(
      Builders<Chat>.Filter.Eq(c => c.Id, objectId.ToString())
      );

    var updateDataBuilder = Builders<Chat>.Update;
    var updates = new List<UpdateDefinition<Chat>>();

    if (!string.IsNullOrWhiteSpace(updatechat.Name))
      updates.Add(updateDataBuilder.Set(c => c.Name, updatechat.Name));

    if (!string.IsNullOrWhiteSpace(updatechat.Description))
      updates.Add(updateDataBuilder.Set(c => c.Description, updatechat.Description));

    updates.Add(updateDataBuilder.Set(c => c.Is_Private, updatechat.Is_Private));

    if (updates.Count == 0) {
      return false;
    }

    var update = updateDataBuilder.Combine(updates);
    var result = await chats.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      return false;
    }
        return true;
  }

  public async Task<bool> DeleteAsync(string org_id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }
    var chats = GetChatCollection(org_id);
    var filter = Builders<Chat>.Filter.And(Builders<Chat>.Filter.Eq(c => c.Id, objectId.ToString())
    );
    var result = await chats.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
