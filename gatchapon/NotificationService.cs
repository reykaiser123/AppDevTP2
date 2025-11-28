using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace gatchapon
{
    public class NotificationService
    {
        public void ScheduleDailyReminder()
        {
            // 1. Create the "Check-In" Notification
            var request = new NotificationRequest
            {
                NotificationId = 100,
                Title = "📅 Don't lose your streak!",
                Description = "Marisol is waiting! Come claim your daily gold.",
                ReturningData = "checkin_reminder", // Data to handle when clicked
                Schedule = new NotificationRequestSchedule
                {
                    // Schedule for 24 hours from now (or set a specific time)
                    NotifyTime = DateTime.Now.AddDays(1),
                    RepeatType = NotificationRepeat.Daily // Repeat every day until they click it
                }
            };

            // 2. Send/Schedule it
            LocalNotificationCenter.Current.Show(request);
        }

        public void ScheduleTaskReminder()
        {
            // 2. Create the "3 Tasks" Reminder
            var request = new NotificationRequest
            {
                NotificationId = 101,
                Title = "⚔️ 3 Tasks Left!",
                Description = "You're so close to your 300 Gold reward. Finish them now!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddHours(4) // Remind them in 4 hours if they leave
                }
            };

            LocalNotificationCenter.Current.Show(request);
        }

        public void CancelTaskReminder()
        {
            // Call this when they actually finish the 3 tasks so we don't annoy them!
            LocalNotificationCenter.Current.Cancel(101);
        }
    }
}