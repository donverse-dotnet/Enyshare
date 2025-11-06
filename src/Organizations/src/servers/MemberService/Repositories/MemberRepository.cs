using MongoDB.Driver;
using MongoDB.Bson;
using Grpc.Core;

namespace MemberService.Repositories {
  public class MemberRepository : IMemberRepository {
    private readonly IMongoCollection<MemberEntity> _membersCollection;
    private readonly IMongoClient _client;
    public MemberRepository(IMongoClient mongoClient) {
      _client = mongoClient;
      var database = mongoClient.GetDatabase("PoccoDb");
      _membersCollection = database.GetCollection<MemberEntity>("Members");
    }

    public async Task<List<MemberEntity>> GetListAsync(string org_id) {
      var collection = GetMongoCollection(org_id);

      var filter = Builders<MemberEntity>.Filter.Empty;

      return await collection.Find(filter).ToListAsync();
    }

    private FilterDefinition<MemberEntity> CreateFilter(string Id) {
      if (!ObjectId.TryParse(Id, out _)) {
        throw new ArgumentException("Invalid id or chatId format");
      }

      return Builders<MemberEntity>.Filter.And(
      Builders<MemberEntity>.Filter.Eq(c => c.Id, Id)
      );
    }

    private IMongoCollection<MemberEntity> GetMongoCollection(string org_id) {
      if (!ObjectId.TryParse(org_id, out _)) {
        throw new ArgumentException("Invalid id or Id format");
      }

      var db = _client.GetDatabase(org_id);
      return db.GetCollection<MemberEntity>("Members");
    }

    public async Task<MemberEntity> CreateAsync(string org_id, MemberEntity member) {
      var members = GetMongoCollection(org_id);
      await members.InsertOneAsync(member);

      var latestMember = await GetByIdAsync(org_id, member.Id);

      if (latestMember is null) {
        throw new RpcException(new Status(StatusCode.Internal, $"Member creation failed for {org_id}"));
      }

      return latestMember;
    }

    public async Task<MemberEntity> GetByIdAsync(string org_id, string Id) {
      var members = GetMongoCollection(org_id);
      var filter = CreateFilter(Id);

      return await members.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> TryUpdateAsync(string org_id, string Id, MemberEntity updatemember) {
      var latestMember = await GetByIdAsync(org_id, Id);

      var isNicknameChanged = updatemember.IsNicknameChanged(latestMember.Nickname);

      if (!isNicknameChanged) {
        return false;
      }

      var updateDataBuilder = Builders<MemberEntity>.Update;
      var updates = new List<UpdateDefinition<MemberEntity>>();

      if (isNicknameChanged)
        updates.Add(updateDataBuilder.Set(c => c.Nickname, updatemember.Nickname));

      updates.Add(updateDataBuilder.Set(c => c.Is_Private, updatemember.Is_Private));

      var update = updateDataBuilder.Combine(updates);
      var members = GetMongoCollection(org_id);
      var filter = CreateFilter(Id);
      var result = await members.UpdateOneAsync(filter, update);

      return result.ModifiedCount != 0;
    }


    public async Task<bool> DeleteAsync(string org_id, string Id) {
      var members = GetMongoCollection(org_id);
      var filter = CreateFilter(Id);
      var result = await members.DeleteOneAsync(filter);
      return result.DeletedCount > 0;
    }
  }
}
