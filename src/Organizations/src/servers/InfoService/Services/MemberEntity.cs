namespace InfoService.Services;

public class MemberEntity
{
  public string Id { get; set; }
  public string OrganizationId { get; set; }
  public string UserId { get; set; }
  public string Role { get; set; }
  public DataTime JoinedAt { get; set; }
}
