namespace Pocco.APIClient.Core.Models;

public class OrganizationOnItemDeletedModel(string OrganizationId, string ItemId, string onItemId) : OrganizationItemDeletedModel(OrganizationId, ItemId) {
    public string OnItemId = onItemId;
}
