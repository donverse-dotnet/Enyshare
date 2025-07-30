
using MongoDB.Driver;

using Pocco.Svc.Accounts.Protos.Ui;
using Pocco.Svc.Accounts.Models;

namespace Pocco.Svc.Accounts.Mappers;

public static class AccountSettingMapper {
    public static Settings ToModel(UiSetting proto, string userId) {
        return new Settings {
            UserId = userId,

            Layout = ToLayout(proto.Layout ?? new Protos.Ui.LayoutSetting()),
            Accessibility = ToAccessibility(proto.Accessibility ?? new Protos.Ui.AccessibilitySetting()),
            Notifications = ToNotifications(proto.Notifications ?? new Protos.Ui.NotificationSetting()),
            Interaction = ToInteraction(proto.Interaction ?? new Protos.Ui.InteractionSetting()),
            ExtensionApps = ToExtensionApps(proto.ExtensionApps ?? new Protos.Ui.ExtensionAppSetting())
        };
    }

    public static Models.LayoutSetting ToLayout(Protos.Ui.LayoutSetting proto) => new Models.LayoutSetting {
        NavigationMode = proto.NavigationMode,                  //tabやsidebarなど
        ThemeMode = proto.ThemeMode,                            //テーマ
    };

    public static Models.AccessibilitySetting ToAccessibility(Protos.Ui.AccessibilitySetting proto) => new Models.AccessibilitySetting {
        HighContrast = proto.HighContrast,                //白黒強調などに切り替えるアクセシビリティ
        KeyboardNavigation = proto.KeyboardNavigation,    //キーボード操作
        ContrastLevel = proto.ContrastLevel               //細かな色覚調整
    };

    public static Models.NotificationSetting ToNotifications(Protos.Ui.NotificationSetting proto) => new Models.NotificationSetting {
        Email = proto.Email,                    //Eメール通知の有効化
        Push = proto.Push,                      //スマホなどのプッシュ通知の有効化
        ShowBadge = proto.ShowBadge,            //通知アイコンに赤丸を表示するか
        UnreadCount = proto.UnreadCount         //未読通知件数
    };

    public static Models.InteractionSetting ToInteraction(Protos.Ui.InteractionSetting proto) => new Models.InteractionSetting {
        EnableVoting = proto.EnableVoting,              //投票機能
        EnableReactions = proto.EnableReactions,        //いいねなどの反応機能
        ReactionTypes = proto.ReactionTypes.ToList()    //リアクション一覧
    };

    public static Models.ExtensionAppSetting ToExtensionApps(Protos.Ui.ExtensionAppSetting proto) => new Models.ExtensionAppSetting {
        ChatbotEnabled = proto.ChatbotEnabled,            //AIチャット機能の有効化
        MiniGames = proto.MiniGames.ToList(),             //UI内に表示する小型ゲーム
        ExternalAppUrls = proto.ExternalAppUrls.ToDictionary(p => p.Key, p => p.Value)    //カスタム外部アプリへのリンク
    };
}



