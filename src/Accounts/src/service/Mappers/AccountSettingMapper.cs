using System;
using System.Security.Cryptography.X509Certificates;

using MongoDB.Driver;

using Pocco.Svc.Accounts.Protos.Ui;
using Pocco.Svc.Accounts.Users;
using Pocco.Svc.Accounts.UsersSettings;

namespace Pocco.Svc.Accounts.UiMapper {
    public static class AccountSettingMapper {
        public static Setting ToModel(Setting proto) {
            return new Setting {
                Layout = ToLayout(Protos.Ui.Layout),
                Accessibility = ToAccessibility(proto.Accessibility),
                Notifications = ToNotifications(proto.Notifications),
                Interaction = ToInteraction(proto.Interaction),
                ExtensionApps = ToExtensionApps(proto.ExtensionApps)
            };
        }

        public static Protos.Ui.LayoutSetting ToLayout(Protos.Ui.LayoutSetting proto) => new Protos.Ui.LayoutSetting {
            NavigationMode = proto.NavigationMode,                  //tabやsidebarなど
            ThemeMode = proto.ThemeMode,                            //テーマ
            Responsive = new Protos.Ui.ResponsiveSetting {
                Enable = proto.Responsive.Enable,  //レスポンシブ対応するか
                PreferredDevices = proto.Responsive.PreferredDevices.ToList() //優先デバイス
            },
            CustomTheme = new Protos.Ui.CustomThemeSetting {
                PrimaryColor = proto.CustomTheme.PrimaryColor,    //主に使用される色
                FontFamily = proto.CustomTheme.FontFamily         //UI全体で使われるフォント
            }
        };

        public static Protos.Ui.AccessibilitySetting ToAccessibility(Protos.Ui.AccessibilitySetting proto) => new Protos.Ui.AccessibilitySetting {
            HighContrast = proto.HighContrast,                //白黒強調などに切り替えるアクセシビリティ
            KeyboardNavigation = proto.KeyboardNavigation,    //キーボード操作
            ContrastLevel = proto.ContrastLevel               //細かな色覚調整
        };

        public static Protos.Ui.NotificationSetting ToNotifications(Protos.Ui.NotificationSetting proto) => new Protos.Ui.NotificationSetting {
            Email = proto.Email,                    //Eメール通知の有効化
            Push = proto.Push,                      //スマホなどのプッシュ通知の有効化
            ShowBadge = proto.ShowBadge,            //通知アイコンに赤丸を表示するか
            UnreadCount = proto.UnreadCount         //未読通知件数
        };

        public static Protos.Ui.InteractionSetting ToInteraction(Protos.Ui.InteractionSetting proto) => new Protos.Ui.InteractionSetting {
            EnableVoting = proto.EnableVoting,              //投票機能
            EnableReactions = proto.EnableReactions,        //いいねなどの反応機能
            ReactionTypes = proto.ReactionTypes.ToList()    //リアクション一覧
        };

        public static Protos.Ui.ExtensionAppSetting ToExtensionApps(Protos.Ui.ExtensionAppSetting proto) => new Protos.Ui.ExtensionAppSetting {
            ChatbotEnabled = proto.ChatbotEnabled,            //AIチャット機能の有効化
            MiniGames = proto.MiniGames.ToList(),             //UI内に表示する小型ゲーム
            ExternalAppUrls = proto.ExternalAppUrls.ToDictionary(p => p.Key, p => p.Value)    //カスタム外部アプリへのリンク
        };
    }
}



