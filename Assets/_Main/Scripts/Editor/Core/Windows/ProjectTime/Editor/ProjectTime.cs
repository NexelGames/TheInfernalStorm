using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Text;

namespace Analyse.Time {
    [InitializeOnLoad]
    public static class ProjectTime {
        private const float RefreshFileAfterSeconds = 60f;

        private readonly static string RefreshedFileTimeStampKey = "ProjectTimeFileRefreshedStamp";
        private readonly static string DataPath = Path.Combine(Application.dataPath, "..", "Library/ProjectTime.json");

        private static ProjectTimeData _data;
        public static ProjectTimeData Data {
            get {
                if (_data == null) {
                    LoadData();
                }
                return _data;
            }
        }

        public static float RefreshedFileTimeStamp => EditorPrefs.GetFloat(RefreshedFileTimeStampKey, 0f);


        static ProjectTime() {
            UnsubscribeEvents();
            SubscribeEvents();
        }

        public static void GetInfo() {
            if (_data == null) {
                LoadData();
            }
            Debug.Log(_data.TotalTimeSeconds);
            StringBuilder byDayInfo = new StringBuilder();
            foreach(var keyValue in _data.TimeByDays) {
                byDayInfo.Append(keyValue.Key.ToString() + keyValue.Value);
            }
            Debug.Log(byDayInfo.ToString());
        }

        private static void RefreshData(float refreshTimeStamp) {
            if (_data == null) {
                LoadData();
            }

            float newStamp = (float)EditorApplication.timeSinceStartup;
            float addedTime = newStamp - refreshTimeStamp;

            _data.TotalTimeSeconds += addedTime;
            _data.AddTime(GetTodayKey(), addedTime);

            SaveData(newStamp);
        }

        private static void OnEditorQuit() {
            RefreshData(RefreshedFileTimeStamp);
            EditorPrefs.DeleteKey(RefreshedFileTimeStampKey);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange) {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode) {

                float refreshTimeStamp = RefreshedFileTimeStamp;
                if (EditorApplication.timeSinceStartup - refreshTimeStamp > RefreshFileAfterSeconds) {
                    RefreshData(refreshTimeStamp);
                }
            }
        }

        private static void LoadData() {
            if (File.Exists(DataPath) == false) {
                _data = new ProjectTimeData();
                SaveData(0f);
            }
            _data = JsonConvert.DeserializeObject<ProjectTimeData>(File.ReadAllText(DataPath));
        }

        /// <summary>
        /// Time in project for today
        /// </summary>
        public static float GetEditorTimeForToday() {
            if (_data == null) {
                LoadData();
            }

            double todayTimeInProject;
            _data.TimeByDays.TryGetValue(GetTodayKey(), out todayTimeInProject);
            todayTimeInProject += EditorApplication.timeSinceStartup - RefreshedFileTimeStamp;
            return (float)todayTimeInProject;
        }

        public static DateTime GetTodayKey() {
            var currentTime = DateTime.Now;
            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
        }

        private static void SaveData(float newTimeStamp) {
            EditorPrefs.SetFloat(RefreshedFileTimeStampKey, newTimeStamp);
            File.WriteAllText(DataPath, JsonConvert.SerializeObject(_data, Formatting.Indented));
        }

        private static void SubscribeEvents() {
            EditorApplication.quitting += OnEditorQuit;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private static void UnsubscribeEvents() {
            EditorApplication.quitting -= OnEditorQuit;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
}