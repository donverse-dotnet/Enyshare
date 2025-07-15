using MongoDB.Bson;

namespace Pocco.Svc.Accounts.Settings{
    public class Settings{
        public ObjectId id { get; set; }
        public ObjectId userid { get; set; }
        
    }
}