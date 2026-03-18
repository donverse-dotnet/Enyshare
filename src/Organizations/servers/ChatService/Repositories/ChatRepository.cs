using MongoDB.Driver;
using MongoDB.Bson;

using Pocco.Svc.Chats.Models;
using Grpc.Core;

namespace Pocco.Svc.Chats.Ripositories;

public class ChatRepository : IChatRepository {
  private readonly IMongoClient _client;

  public ChatRepository(IMongoClient client) {
    _client = client;
  }

  public async Task<List<Chat>> GetListAsync(string orgId) {
    var collection = GetChatCollection(orgId);

    var filter = Builders<Chat>.Filter.Empty;

    return await collection.Find(filter).ToListAsync();
  }

  private FilterDefinition<Chat> CreateFilter(string chatId) {
    if (!ObjectId.TryParse(chatId, out _)) {
      throw new ArgumentException("Invalid id or chatId format");
    }

    return Builders<Chat>.Filter.And(
    Builders<Chat>.Filter.Eq(c => c.Id, chatId)
    );
  }


  private IMongoCollection<Chat> GetChatCollection(string orgId) {
    if (!ObjectId.TryParse(orgId, out _)) {
      throw new ArgumentException("Invalid id or chatId format");
    }

    var db = _client.GetDatabase(orgId);
    return db.GetCollection<Chat>("Chats");
  }

  public async Task<Chat> CreateAsync(string orgId, Chat chat) {
    var chats = GetChatCollection(orgId);
    await chats.InsertOneAsync(chat);

    var latestChat = await GetByIdAsync(orgId, chat.Id);

    if (latestChat is null) {
      throw new RpcException(new Status(StatusCode.Internal, $"Chat creation failed for {orgId}"));
    }

    return latestChat;
  }

  public async Task<Chat> GetByIdAsync(string orgId, string chatId) {
    var chats = GetChatCollection(orgId);
    var filter = CreateFilter(chatId);

    return await chats.Find(filter).FirstOrDefaultAsync();

  }

  public async Task<bool> TryUpdateAsync(string orgId, string chatId, Chat newChat) {
    var latestChat = await GetByIdAsync(orgId, chatId);

    var isNameChanged = newChat.IsDescriptionChanged(latestChat.Name);
    var isDescriptionChanged = newChat.IsDescriptionChanged(latestChat.Description);

    if (!isNameChanged && isDescriptionChanged) {
      return false;
    }

    var updateDataBuilder = Builders<Chat>.Update;
    var updates = new List<UpdateDefinition<Chat>>();

    if (isNameChanged)
      updates.Add(updateDataBuilder.Set(c => c.Name, newChat.Name));

    if (isDescriptionChanged)
      updates.Add(updateDataBuilder.Set(c => c.Description, newChat.Description));

    updates.Add(updateDataBuilder.Set(c => c.IsPrivate, newChat.IsPrivate));

    var update = updateDataBuilder.Combine(updates);
    var chats = GetChatCollection(orgId);
    var filter = CreateFilter(chatId);
    var result = await chats.UpdateOneAsync(filter, update);

    return result.ModifiedCount != 0;
  }

  public async Task<bool> DeleteAsync(string orgId, string chatId) {
    var chats = GetChatCollection(orgId);
    var filter = CreateFilter(chatId);
    var result = await chats.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
