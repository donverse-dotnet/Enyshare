namespace MemberService.Repositories {
    public interface IMemberRepository {
        Task<MemberEntity> CreateAsync(string org_id, MemberEntity member);
        Task<MemberEntity> GetByIdAsync(string org_id, string id);
        Task<bool> TryUpdateAsync(string org_id, string id, MemberEntity updatemember);
        Task<bool> DeleteAsync(string org_id, string id);
    }
}
