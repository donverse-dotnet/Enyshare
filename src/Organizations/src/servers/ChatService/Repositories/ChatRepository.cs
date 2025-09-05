using MongoDB.Driver;

using Pocco.Svc.Chats.Models;

public class ChatRepository : IChatRepository {
    private readonly IMongoCollection<Chat> _collection;

    public ChatRepository(IMongoDatabase database) {
        _collection = database.GetCollection<Chat>("Chats");
    }

    public async Task<Chat> CreateAsync(Chat chat) {
        if (chat == null)
            throw new ArgumentNullException(nameof(chat));

        await _collection.InsertOneAsync(chat);
        return chat;
    }

    public async Task<Chat?> GetByIdAsync(string org_id, string id) {
        if (string.IsNullOrEmpty(org_id))
            throw new ArgumentException("org_id must not be null or empty", nameof(org_id));
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("id must not be null or empty", nameof(id));

        var filter = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.Eq("_id", id),
            Builders<Chat>.Filter.Eq("org_id", org_id)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();

    }

    public async Task<Chat?> UpdateAsync(string org_id, string id, Chat chat) {
        if (string.IsNullOrEmpty(org_id))
            throw new ArgumentException("org_id must not be null or empty", nameof(org_id));
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("id must not be null or empty", nameof(id));
        if (chat == null)
            throw new ArgumentNullException(nameof(chat));

        var filter = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.Eq("_id", id),
            Builders<Chat>.Filter.Eq("org_id", org_id)
        );

        var update = Builders<Chat>.Update
        .Set(c => c.Name, chat.Name)
        .Set(c => c.Description, chat.Description)
        .Set(c => c.Is_Private, chat.Is_Private);

        var result = await _collection.UpdateOneAsync(filter, update);
        if (result.ModifiedCount > 0) {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        return null;
    }

    public async Task<bool> DeleteAsync(string org_id, string id) {
        if (string.IsNullOrEmpty(org_id))
            throw new ArgumentException("org_id must not be null or empty", nameof(org_id));
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("id must not be null or empty", nameof(id));

        var filter = Builders<Chat>.Filter.And(
            Builders<Chat>.Filter.Eq("_id", id),
            Builders<Chat>.Filter.Eq("org_id", org_id)
        );
        var result = await _collection.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}