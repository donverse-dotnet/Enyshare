using MongoDB.Bson;
using InfoService.Services;

namespace InfoService.Repositories
{
    public interface IInfoRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(Object id);
        Task InsertAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(ObjectId id);
    }
}