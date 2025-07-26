
using MongoDB.Driver;

using Pocco.Svc.Accounts.Services.Protos.Ui;

using Pocco.Svc.Accounts.Services.UsersSettings;

namespace Pocco.Svc.Accounts.Services.UiMapper {
    public static class AccountSettingMapper {
        public static Setting ToModel(UiSetting proto, string userId) {
            return new Setting {
                UserId = userId,

                Layout = ToLayout(proto.Layout ?? new Protos.Ui.LayoutSetting()),
                Accessibility = ToAccessibility(proto.Accessibility ?? new Protos.Ui.AccessibilitySetting()),
                Notifications = ToNotifications(proto.Notifications ?? new Protos.Ui.NotificationSetting()),
                Interaction = ToInteraction(proto.Interaction ?? new Protos.Ui.InteractionSetting()),
                ExtensionApps = ToExtensionApps(proto.ExtensionApps ?? new Protos.Ui.ExtensionAppSetting())
            };
        }

        public static UsersSettings.LayoutSetting ToLayout(Protos.Ui.LayoutSetting proto) => new UsersSettings.LayoutSetting {
            NavigationMode = proto.NavigationMode,                  //tabやsidebarなど
            ThemeMode = proto.ThemeMode,                            //テーマ
            Responsive = new UsersSettings.ResponsiveSetting {
                Enable = proto.Responsive.Enable,  //レスポンシブ対応するか
                PreferredDevices = proto.Responsive.PreferredDevices.ToList() //優先デバイス
            },
            CustomTheme = new UsersSettings.CustomThemeSetting {
                PrimaryColor = proto.CustomTheme.PrimaryColor,    //主に使用される色
                FontFamily = proto.CustomTheme.FontFamily         //UI全体で使われるフォント
            }
        };

        public static UsersSettings.AccessibilitySetting ToAccessibility(Protos.Ui.AccessibilitySetting proto) => new UsersSettings.AccessibilitySetting {
            HighContrast = proto.HighContrast,                //白黒強調などに切り替えるアクセシビリティ
            KeyboardNavigation = proto.KeyboardNavigation,    //キーボード操作
            ContrastLevel = proto.ContrastLevel               //細かな色覚調整
        };

        public static UsersSettings.NotificationSetting ToNotifications(Protos.Ui.NotificationSetting proto) => new UsersSettings.NotificationSetting {
            Email = proto.Email,                    //Eメール通知の有効化
            Push = proto.Push,                      //スマホなどのプッシュ通知の有効化
            ShowBadge = proto.ShowBadge,            //通知アイコンに赤丸を表示するか
            UnreadCount = proto.UnreadCount         //未読通知件数
        };

        public static UsersSettings.InteractionSetting ToInteraction(Protos.Ui.InteractionSetting proto) => new UsersSettings.InteractionSetting {
            EnableVoting = proto.EnableVoting,              //投票機能
            EnableReactions = proto.EnableReactions,        //いいねなどの反応機能
            ReactionTypes = proto.ReactionTypes.ToList()    //リアクション一覧
        };

        public static UsersSettings.ExtensionAppSetting ToExtensionApps(Protos.Ui.ExtensionAppSetting proto) => new UsersSettings.ExtensionAppSetting {
            ChatbotEnabled = proto.ChatbotEnabled,            //AIチャット機能の有効化
            MiniGames = proto.MiniGames.ToList(),             //UI内に表示する小型ゲーム
            ExternalAppUrls = proto.ExternalAppUrls.ToDictionary(p => p.Key, p => p.Value)    //カスタム外部アプリへのリンク
        };
    }
}



