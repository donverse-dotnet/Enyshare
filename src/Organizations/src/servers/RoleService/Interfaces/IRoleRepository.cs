using Pocco.Svc.Roles.Models;

public interface IRoleRepository {
  Task<Role> GetByIdAsync(string orgId, string roleId);
  Task<Role> CreateAsync(string orgId, Role role);
  Task<bool> TryUpdateAsync(string orgId, string roleId, Role newRole);
  Task<bool> DeleteAsync(string orgId, string roleId);
}
