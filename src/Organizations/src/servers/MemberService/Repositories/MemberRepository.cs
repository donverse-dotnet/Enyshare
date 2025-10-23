using MongoDB.Driver;
using MongoDB.Bson;
using MemberService.Services;
using Grpc.Core;

namespace MemberService.Repositories {
    public class MemberRepository : IMemberRepository {
        private readonly IMongoClient _client;
        public MemberRepository(IMongoClient client) {
            _client = client;
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

        public async Task UpdateAsync(MemberEntity entity) {
            var filter = Builders<MemberEntity>.Filter.Eq(x => x.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(string id) {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
