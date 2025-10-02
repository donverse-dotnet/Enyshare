using MongoDB.Driver;
using MongoDB.Bson;
using InfoService.Services;

namespace InfoService.Repositories
{
    public class InfoRepository : IInfoRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public InfoRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Product>("Products");
        }

        public async Task<List<Product>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<Product> GetByIdAsync(Object id) =>
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task InsertAsync(Product product) =>
            await _collection.InsertOneAsync(product);

        public async Task UpdateAsync(Product product) =>
            await _collection.ReplaceOneAsync(product => product.Id == product.Id, product);

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(p => p.Id == id);
    }
}