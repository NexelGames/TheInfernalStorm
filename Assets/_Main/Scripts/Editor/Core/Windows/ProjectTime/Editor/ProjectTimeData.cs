using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Analyse.Time {
    [Serializable]
    public class ProjectTimeData {
        public float TotalTimeSeconds = 0f;

        public Dictionary<DateTime, double> TimeByDays = new Dictionary<DateTime, double>();
        public void AddTime(DateTime dayMonthYear, double addTime) {
            if (TimeByDays.ContainsKey(dayMonthYear)) {
                TimeByDays[dayMonthYear] += addTime;
            }
            else {
                TimeByDays.Add(dayMonthYear, addTime);
            }
        }

        public string GetLast30DaysInfo() {
            StringBuilder info = new StringBuilder(31);

            var curTime = DateTime.Now;
            var timeByDaysWithoutToday = new Dictionary<DateTime, double>(TimeByDays);
            timeByDaysWithoutToday.Remove(ProjectTime.GetTodayKey());

            info.Append(LogFormat((int)ProjectTime.GetEditorTimeForToday(), curTime));

            foreach (var keyPair in timeByDaysWithoutToday.Reverse()) {
                TimeSpan timeSpan = curTime - keyPair.Key;
                if (timeSpan.Days < 31) {
                    DateTime sessionDate = curTime.AddDays(-timeSpan.Days);
                    int seconds = (int)keyPair.Value;
                    info.Append(LogFormat(seconds, sessionDate));
                }
                else {
                    break;
                }
            }
            return info.ToString();
        }

        private string LogFormat(int seconds, DateTime date) {
            return $"\n{date.ToString("dddd dd MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))} : {GetTimeString(seconds)}";
        }

        public static string GetTimeString(int seconds) {
            int minutes = seconds / 60;
            int hours = minutes / 60;

            if (minutes == 0) return $"{seconds}сек.";
            if (hours == 0) return $"{minutes % 60}мин. и {seconds % 60}сек.";
            return $"{hours}ч. {minutes % 60}мин. {seconds % 60}сек.";
        }
    }
}