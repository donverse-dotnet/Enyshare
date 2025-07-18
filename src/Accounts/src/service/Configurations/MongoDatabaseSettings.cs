using System;

namespace Pocco.Svc.Accounts.Settings {
    public class MongoDatabaseSettings {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public MongoCollection Collection { get; set; } = null!;
    }
    public class MongoCollection {
        public string Accounts { get; set; } = null;
        public string AccountSettings { get; set; } = null;
    }
}
