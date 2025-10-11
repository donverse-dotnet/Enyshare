using Pocco.Svc.Chats.Models;

public interface IChatRepository {
    Task<Chat> CreateAsync(string org_id, Chat chat);
    Task<Chat> GetByIdAsync(string org_id, string id);
    Task<Chat> UpdateAsync(string org_id, Chat updatechat);
    Task<bool> DeleteAsync(string org_id, string id);
}
