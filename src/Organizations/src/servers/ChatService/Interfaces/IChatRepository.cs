using Pocco.Svc.Chats.Models;

public interface IChatRepository {
    Task<Chat> CreateAsync(Chat chat);
    Task<Chat?> GetByIdAsync(string org_id, string id);
    Task<Chat?> UpdateAsync(string org_id, string id, Chat chat);
    Task<bool> DeleteAsync(string org_id, string id);
}