using MongoDB.Bson;
using InfoService.Services;

namespace InfoService.Repositories
{
    public interface IInfoRepository
    {
      Task<OrganizationEntity> FindByIdAsync(string id);
      Task InsertAsync(OrganizationEntity entity);
      Task UpdateAsync(OrganizationEntity entity);
      Task DeleteAsync(string id);
    }
}
