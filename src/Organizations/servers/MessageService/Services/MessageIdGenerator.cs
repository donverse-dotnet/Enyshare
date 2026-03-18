namespace MessageService.Services;

public class MessageIdGenerator {
  public const string OrgMsgPrefix = "org-msg";

  public static string GenerateOrganizationMessageId() {
    // {prefix}_{unix_timestamp}_{UUIDv4}
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var uuid = Guid.NewGuid().ToString("N"); // Generate a UUID without hyphens
    return $"{OrgMsgPrefix}_{timestamp}_{uuid}";
  }
}
