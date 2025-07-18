using MongoDB.Bson;

namespace Pocco.Svc.Accounts.UsersSettings{
    public class Setting{
        public ObjectId id { get; set; }
        public ObjectId userid { get; set; }
        
    }
}