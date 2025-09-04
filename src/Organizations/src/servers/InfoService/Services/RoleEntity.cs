namespace InfoService.Services;

public class RoleEntity
{
  public string Id { get; set; }
  public string OrganizationId { get; set; }
  public string Name { get; set; }
  public string[] Permissions { get; set; }
}
