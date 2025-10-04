using MongoDB.Driver;
using MongoDB.Bson;
using InfoService.Services;

namespace InfoService.Repositories
{
  public class InfoRepository : IInfoRepository
  {
    private readonly IMongoCollection<OrganizationEntity> _collection;
    public InfoRepository(IMongoDatabase database)
    {
      _collection = database.GetCollection<OrganizationEntity>("infos");
    }

    public async Task<OrganizationEntity> FindByIdAsync(string id)
    {
      return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task InsertAsync(OrganizationEntity entity)
    {
      await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(OrganizationEntity entity)
    {
      var filter = Builders<OrganizationEntity>.Filter.Eq(x => x.Id, entity.Id);
      await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
      await _collection.DeleteOneAsync(x => x.Id == id);
    }
  }
}
