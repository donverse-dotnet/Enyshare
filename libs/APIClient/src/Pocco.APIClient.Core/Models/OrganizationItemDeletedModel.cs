namespace Pocco.APIClient.Core.Models;

public class OrganizationItemDeletedModel(string OrganizationId, string ItemId) {
    public string OrganizationId = OrganizationId;
    public string ItemId = ItemId;
}
