using MongoDB.Bson;
using MemberService.Services;

namespace MemberService.Repositories {
    public interface IMemberRepository {
        Task<MemberEntity> CreateAsync(string org_id, MemberEntity member);
        Task<MemberEntity> GetByIdAsync(string org_id, string id);
        Task<MemberEntity> FindByIdAsync(string id);
        Task InsertAsync(MemberEntity entity);
        Task UpdateAsync(MemberEntity entity);
        Task DeleteAsync(string id);
    }
}
