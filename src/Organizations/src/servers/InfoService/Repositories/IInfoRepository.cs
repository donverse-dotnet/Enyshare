using MongoDB.Bson;
using InfoService.Services;

namespace InfoService.Repositories
{
    public interface IInfoRepository
    {
      Task<InfoEntity> FindByIdAsync(string id);
      Task InsertAsync(InfoEntity entity);
      Task UpdateAsync(InfoEntity entity);
      Task DeleteAsync(string id);
    }
}
