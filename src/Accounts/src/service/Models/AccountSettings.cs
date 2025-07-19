using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Accounts.UsersSettings {
    public class Setting {
        public ObjectId id { get; set; }
        public ObjectId userid { get; set; }

        [BsonElement("layout")]
        public LayoutSetting Layout { get; set; }

        [BsonElement("accessibility")]
        public AccessibilitySetting Accessibility { get; set; }

        [BsonElement("notifications")]
        public NotificationSetting Notifications { get; set; }

        [BsonElement("interaction")]
        public InteractionSetting Interaction { get; set; }

        [BsonElement("extensionApps")]
        public ExtensionAppSetting ExtensionApps { get; set; }
    }

public class LayoutSetting{
    [BsonElement("responsive")]
    public ResponsiveSetting Responsive { get; set; }

    [BsonElement("navigationMode")]
    public string NavigationMode { get; set; } // "tab" or "sidebar"

    [BsonElement("themeMode")]
    public string ThemeMode { get; set; } // "light" or "dark"

    [BsonElement("customTheme")]
    public CustomThemeSetting CustomTheme { get; set; }
}

    public class ResponsiveSetting {
        [BsonElement("enable")]
        public bool Enable { get; set; }

        [BsonElement("preferredDevices")]
        public List<string> PreferredDevices { get; set; } // ["pc", "mobile", "tablet"]
    }

public class CustomThemeSetting
{
    [BsonElement("primaryColor")]
    public string PrimaryColor { get; set; }

    [BsonElement("fontFamily")]
    public string FontFamily { get; set; }
}

public class AccessibilitySetting
{
    [BsonElement("fontSize")]
    public string FontSize { get; set; }

    [BsonElement("highContrast")]
    public bool HighContrast { get; set; }

    [BsonElement("keyboardNavigation")]
    public bool KeyboardNavigation { get; set; }

    [BsonElement("contrastLevel")]
    public string ContrastLevel { get; set; }
}

public class NotificationSetting
{
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

public class InteractionSetting{
    [BsonElement("enableVoting")]
    public bool EnableVoting { get; set; }

    [BsonElement("enableReactions")]
    public bool EnableReactions { get; set; }

    [BsonElement("reactionTypes")]
    public List<string> ReactionTypes { get; set; }
    }

public class ExtensionAppSetting {
        [BsonElement("chatbotEnabled")]
        public bool ChatbotEnabled { get; set; }

        [BsonElement("miniGames")]
        public List<string> MiniGames { get; set; }

        [BsonElement("externalAppUrls")]
        public Dictionary<string, string> ExternalAppUrls { get; set; }
    }
}
