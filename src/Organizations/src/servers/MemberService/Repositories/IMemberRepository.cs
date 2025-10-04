using MongoDB.Bson;
using MemberService.Services;

namespace MemberService.Repositories {
    public interface IMemberRepository {
        Task<MemberEntity> FindByIdAsync(string id);
        Task InsertAsync(MemberEntity entity);
        Task UpdateAsync(MemberEntity entity);
        Task DeleteAsync(string id);
    }
}
