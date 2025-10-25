namespace Pocco.Svc.CoreAPI.Models;

public class StreamWriterFilterModel(List<string> topics, List<string> organizationIds, string activeOrganizationId, List<string> activeOrganizationChatIds) {
  public List<string> Topics { get; private set; } = topics;

  public List<string> OrganizationIds { get; private set; } = organizationIds;
  public string ActiveOrganizationId { get; private set; } = activeOrganizationId;
  public List<string> ActiveOrganizationChatIds { get; private set; } = activeOrganizationChatIds;

  public void UpdateTopics(List<string> newTopics) {
    Topics = newTopics;
  }
  public void AddOrganizationId(string organizationId) {
    if (!OrganizationIds.Contains(organizationId)) {
      OrganizationIds.Add(organizationId);
    }
  }
  public void RemoveOrganizationId(string organizationId) {
    OrganizationIds.Remove(organizationId);
  }
  public void UpdateActiveOrganizationId(string newActiveOrganizationId) {
    ActiveOrganizationId = newActiveOrganizationId;
  }

  public void UpdateActiveOrganizationChatIds(List<string> newActiveOrganizationChatIds) {
    ActiveOrganizationChatIds = newActiveOrganizationChatIds;
  }
  public void AddActiveOrganizationChatId(string chatId) {
    if (!ActiveOrganizationChatIds.Contains(chatId)) {
      ActiveOrganizationChatIds.Add(chatId);
    }
  }
  public void RemoveActiveOrganizationChatId(string chatId) {
    ActiveOrganizationChatIds.Remove(chatId);
  }

  public bool MatchesTopic(string topic) {
    return Topics.Contains(topic);
  }

  public bool MatchesOrganizationId(string organizationId) {
    return OrganizationIds.Contains(organizationId);
  }

  public bool MatchesActiveOrganizationId(string organizationId) {
    return ActiveOrganizationId == organizationId;
  }

  public bool MatchesActiveOrganizationChatId(string chatId) {
    return ActiveOrganizationChatIds.Contains(chatId);
  }

  public override string ToString() {
    return $"Topics: [{string.Join(", ", Topics)}], OrganizationIds: [{string.Join(", ", OrganizationIds)}], ActiveOrganizationId: {ActiveOrganizationId}, ActiveOrganizationChatIds: [{string.Join(", ", ActiveOrganizationChatIds)}]";
  }
}
