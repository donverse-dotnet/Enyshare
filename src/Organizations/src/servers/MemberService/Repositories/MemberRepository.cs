using MongoDB.Driver;
using MongoDB.Bson;
using MemberService.Services;

namespace MemberService.Repositories {
    public class MemberRepository : IMemberRepository {
        public readonly IMongoCollection<MemberEntity> _collection;
        public MemberRepository(IMongoDatabase database) {
            _collection = database.GetCollection<MemberEntity>("Members");
        }

        public async Task<MemberEntity> FindByIdAsync(string id) {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(MemberEntity entity) {
            await _collection.InsertOneAsync(entity);
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
