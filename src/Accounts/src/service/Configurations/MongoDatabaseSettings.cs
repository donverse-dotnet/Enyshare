using System;

namespace Pocco.Svc.Accounts.Settings {
    public class MongoDatabaseSettings {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public MongoCollection Collection { get; set; } = new MongoCollection();
    }
    public class MongoCollection {
        public string Accounts { get; set; } = "Accounts";
        public string AccountSettings { get; set; } = "AccountSettings";
    }
}
