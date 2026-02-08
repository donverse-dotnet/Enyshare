using Pocco.Svc.Chats.Models;

public interface IChatRepository {
  Task<Chat> CreateAsync(string orgId, Chat chat);
  Task<Chat> GetByIdAsync(string orgId, string chatId);
  Task<bool> TryUpdateAsync(string orgId, string chatId, Chat newChat);
  Task<bool> DeleteAsync(string orgId, string chatId);
  Task<List<Chat>> GetListAsync(string orgId);
}
