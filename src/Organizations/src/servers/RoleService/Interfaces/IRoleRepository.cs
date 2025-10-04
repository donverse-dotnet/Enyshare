using Pocco.Svc.Roles.Models;

public interface IRoleRepository {
  Task<Role> GetByIdAsync(string org_Id, string id);
  Task<Role> CreateAsync(string org_Id, Role role);
  Task<bool> TryUpdateAsync(string org_Id, string id, Role updaterole);
  Task<bool> DeleteAsync(string org_Id, string id);
}
