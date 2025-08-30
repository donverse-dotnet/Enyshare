using Pocco.Svc.Roles.Models;

public interface IRoleRepository {
    Task<List<Role>> GetAllAsync();
    Task<Role> GetByIdAsync(string id);
    Task CreateAsync(Role role);
    Task UpdateAsync(string id, Role role);
    Task DeleteAsync(string id);
}