using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Accounts.UsersSettings {
    public class Setting {
        public ObjectId id { get; set; }
        public required string UserId { get; set; }

        [BsonElement("layout")]
        public required LayoutSetting Layout { get; set; }

        [BsonElement("accessibility")]
        public required AccessibilitySetting Accessibility { get; set; }

        [BsonElement("notifications")]
        public required NotificationSetting Notifications { get; set; }

        [BsonElement("interaction")]
        public required InteractionSetting Interaction { get; set; }

        [BsonElement("extensionApps")]
        public required ExtensionAppSetting ExtensionApps { get; set; }
    }

    public class LayoutSetting {
        [BsonElement("responsive")]
        public required ResponsiveSetting Responsive { get; set; }

        [BsonElement("navigationMode")]
        public required string NavigationMode { get; set; } // "tab" or "sidebar"

        [BsonElement("themeMode")]
        public required string ThemeMode { get; set; } // "light" or "dark"

        [BsonElement("customTheme")]
        public required CustomThemeSetting CustomTheme { get; set; }
    }

    public class ResponsiveSetting {
        [BsonElement("enable")]
        public bool Enable { get; set; }

        [BsonElement("preferredDevices")]
        public required List<string> PreferredDevices { get; set; } // ["pc", "mobile", "tablet"]
    }

    public class CustomThemeSetting {
        [BsonElement("primaryColor")]
        public required string PrimaryColor { get; set; }

        [BsonElement("fontFamily")]
        public required string FontFamily { get; set; }
    }

    public class AccessibilitySetting {
        [BsonElement("highContrast")]
        public bool HighContrast { get; set; }

        [BsonElement("keyboardNavigation")]
        public bool KeyboardNavigation { get; set; }

        [BsonElement("contrastLevel")]
        public required string ContrastLevel { get; set; }
    }

    public class NotificationSetting {
        [BsonElement("email")]
        public bool Email { get; set; }

        [BsonElement("push")]
        public bool Push { get; set; }

        [BsonElement("sms")]
        public bool Sms { get; set; }

        [BsonElement("showBadge")]
        public bool ShowBadge { get; set; }

        [BsonElement("unreadCount")]
        public int UnreadCount { get; set; }
    }

    public class InteractionSetting {
        [BsonElement("enableVoting")]
        public bool EnableVoting { get; set; }

        [BsonElement("enableReactions")]
        public bool EnableReactions { get; set; }

        [BsonElement("reactionTypes")]
        public required List<string> ReactionTypes { get; set; }
    }

    public class ExtensionAppSetting {
        [BsonElement("chatbotEnabled")]
        public bool ChatbotEnabled { get; set; }

        [BsonElement("miniGames")]
        public required List<string> MiniGames { get; set; }

        [BsonElement("externalAppUrls")]
        public required Dictionary<string, string> ExternalAppUrls { get; set; }
    }
}
