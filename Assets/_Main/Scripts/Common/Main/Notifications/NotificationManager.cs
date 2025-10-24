using Alchemy.Inspector;
using System;
using System.Collections;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#if UNITY_IOS || UNITY_IPHONE
using Unity.Notifications.iOS;
#endif

using UnityEngine;

namespace Game.Core {

    public enum NotificationsIds : byte {
        GameReturn1 = 100,
        GameReturn2 = 101,

        // add here your custom ids
    }

    /// <summary>
    /// Application notification manager
    /// </summary>
    public class NotificationManager {
        private static NotificationManager instance;
        private static NotificationManager Instance {
            get {
                if (instance == null) {
                    instance = new NotificationManager();
                }
                return instance;
            }
        }

        public static readonly string GameSmallIcon = "small_icon";
        public static readonly string GameLargeIcon = "large_icon";

        private NotificationData[] _gameReturnNotifications = new NotificationData[] {
            new NotificationData() { Title = "Come back to game!",
                                     Text = "We need you!",
                                     HasIcons = true,
                                     SmallIcon = "small_icon",
                                     LargeIcon = "large_icon" },
        };

        /// <summary>
        /// Call this method on start of Application to init notifications
        /// </summary>
        public static IEnumerator Create() {
            if (instance != null) {
                yield break;
            }

            yield return Instance.Init();
        }

        public IEnumerator Init() {
#if UNITY_ANDROID
            yield return RequestNotificationPermission();
            Android_CreateScheduledNotificationChannel();
            Android_UpdateGameReturnNotifications();
#elif UNITY_IOS || UNITY_IPHONE
            yield return RequestAuthorization();            
#else
			yield return null;
#endif
        }

        /// <param name="subTitle">Only for iOS. No need for Android (ignored)!</param>
        /// <param name="invokeTime">Time when it should be invoked</param>
        /// <param name="id">Specify id in <see cref="NotificationsIds>"/> and provide it here. <br>
        /// You can cancel notification with <see cref="CancelNotification(NotificationsIds)"/></br></param>
        public static void ScheduleNotification(string title, string text, string subTitle, DateTime invokeTime, NotificationsIds id) {
#if UNITY_ANDROID
            Instance.Android_SendScheduledNotification(title, text, invokeTime, id);
#elif UNITY_IOS || UNITY_IPHONE
            Instance.iOS_SendScheduledNotification(title, text, subTitle, invokeTime, id);
#endif
        }

        public static void ScheduleNotification(NotificationData notificationData, DateTime invokeTime, NotificationsIds id) {
#if UNITY_ANDROID
            Instance.Android_SendScheduledNotification(notificationData, invokeTime, id);
#elif UNITY_IOS || UNITY_IPHONE
            Instance.iOS_SendScheduledNotification(notificationData.Title, notificationData.Text, notificationData.SubTitle, invokeTime, id);
#endif
        }

        public static void CancelNotification(NotificationsIds id) {
#if UNITY_ANDROID
            Instance.Android_CancelNotification(id);
#elif UNITY_IOS || UNITY_IPHONE
            Instance.iOS_CancelNotification(id);
#endif
        }


        #region Android
#if UNITY_ANDROID
        private AndroidNotificationChannel _scheduledNotificationChannel;

        private IEnumerator RequestNotificationPermission() {
            var request = new PermissionRequest();
            while (request.Status == PermissionStatus.RequestPending)
                yield return null;

        }

        private void Android_CreateScheduledNotificationChannel() {
            _scheduledNotificationChannel = new AndroidNotificationChannel() {
                Id = $"scheduled_{Application.productName}_notifications",
                Name = "Scheduled Notifications",
                Importance = Importance.Default,
                Description = "Notifications to remind the player about the game.",
                EnableLights = true,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(_scheduledNotificationChannel);
        }

        private void Android_UpdateGameReturnNotifications() {
            AndroidNotificationCenter.CancelNotification((int)NotificationsIds.GameReturn1);
            AndroidNotificationCenter.CancelNotification((int)NotificationsIds.GameReturn2);

            // here add notifications on game start
            Android_SendScheduledNotification_GameReturn();
        }

        private void Android_CancelNotification(NotificationsIds id) {
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus((int)id);

            if (notificationStatus == NotificationStatus.Scheduled) {
                AndroidNotificationCenter.CancelScheduledNotification((int)id);
            }
            else if (notificationStatus == NotificationStatus.Delivered) {
                // Remove the previously shown notification from the status bar.
                AndroidNotificationCenter.CancelNotification((int)id);
            }
        }

        private void Android_SendScheduledNotification(string title, string text, DateTime invokeTime, NotificationsIds id) {
            var data = new NotificationData() {
                Title = title,
                Text = text,

                HasIcons = true,
                SmallIcon = GameSmallIcon,
                LargeIcon = GameLargeIcon
            };
            Android_SendScheduledNotification(data, invokeTime, id);
        }

        private void Android_SendScheduledNotification(NotificationData data, DateTime invokeTime, NotificationsIds id) {
            AndroidNotification notification = new AndroidNotification();
            Android_SetupNotification(ref notification, data);

            notification.FireTime = invokeTime;
            notification.ShouldAutoCancel = true;
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, _scheduledNotificationChannel.Id, (int)id);
        }

        private void Android_SendScheduledNotification_GameReturn(int waitDaysCount = 2, int fireHourFirst = 12,
                                                          int fireHourSecond = -1, int repeatInterfalDays = 7) {
            var notificationData = _gameReturnNotifications[UnityEngine.Random.Range(0, _gameReturnNotifications.Length)];

            TimeSpan repeatInterval = new TimeSpan(repeatInterfalDays, 0, 0, 0);
            AndroidNotification notification = new AndroidNotification();
            Android_SetupNotification(ref notification, notificationData);

            // set notification fire time
            DateTime fireTime = DateTime.Now.AddDays(waitDaysCount);
            fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day,
                hour: fireHourFirst, minute: 30, second: 0, DateTimeKind.Local);
            notification.FireTime = fireTime;
            notification.RepeatInterval = repeatInterval;
            notification.ShouldAutoCancel = true;

            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, _scheduledNotificationChannel.Id, (int)NotificationsIds.GameReturn1);

            if (fireHourSecond > 0) {
                notificationData = _gameReturnNotifications[UnityEngine.Random.Range(0, _gameReturnNotifications.Length)];
                Android_SetupNotification(ref notification, notificationData);

                fireTime = new DateTime(fireTime.Year, fireTime.Month, fireTime.Day,
                    hour: fireHourSecond, minute: 0, second: 0, DateTimeKind.Local);
                notification.FireTime = fireTime;
                notification.RepeatInterval = repeatInterval;

                AndroidNotificationCenter.SendNotificationWithExplicitID(notification, _scheduledNotificationChannel.Id, (int)NotificationsIds.GameReturn2);
            }
        }

        private void Android_SetupNotification(ref AndroidNotification notification, NotificationData notificationData) {
            notification.Title = notificationData.Title;
            notification.Text = notificationData.Text;

            if (notificationData.HasIcons) {
                notification.SmallIcon = notificationData.SmallIcon;
                notification.LargeIcon = notificationData.LargeIcon;
            }
        }
#endif
        #endregion

        #region iOS
#if UNITY_IOS || UNITY_IPHONE
        IEnumerator RequestAuthorization() {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true)) {
                while (!req.IsFinished) {
                    yield return null;
                };
            }
        }

        private void iOS_SendScheduledNotification(string title, string text, string subTitle, DateTime invokeTime, NotificationsIds id) {
            var timeTrigger = new iOSNotificationTimeIntervalTrigger() {
                TimeInterval = invokeTime - DateTime.Now,
                Repeats = false
            };

            var notification = new iOSNotification() {
                Identifier = id.ToString(),
                Title = title,
                Body = text,
                Subtitle = subTitle,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Badge),
                CategoryIdentifier = "default_category",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
        }

        private void iOS_CancelNotification(NotificationsIds id) {
            iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
        }
#endif
        #endregion

        /// <summary>
        /// Notification info
        /// </summary>
		[System.Serializable]
        public struct NotificationData {
            public string Title;
            [Tooltip("iOS only")]
            public string SubTitle;
            public string Text;

            [FoldoutGroup("Android")] public bool HasIcons;
            [FoldoutGroup("Android")] public string SmallIcon;
            [FoldoutGroup("Android")] public string LargeIcon;
        }
    }
}