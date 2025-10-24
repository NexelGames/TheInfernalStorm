using UnityEditor;
using UnityEngine;

namespace Analyse.Time {
    public class ProjectTimeWindow : EditorWindow {
		private Texture _timeIcon;
		private ProjectTimeData _timeData;
		private float _timeRefreshStamp;
		private float _totalTimeStamp;
		private bool _showMonthByDaySessions = false;
		private Vector2 _windowScrollPosition;

		private double _refreshGUIDelay = 5f;
		private double _refreshGUIStamp;

		private string _sessionTimeString;
		private string _totalTimeString;
		private string _last30DaysTimeString;

        [MenuItem("Window/Analysis/Project Time")]
		private static void CreateWindow() {
			GetWindow<ProjectTimeWindow>("ProjectTimeWindow").Init();
		}

		// Draw the content of window.
		private void OnGUI() {
			_windowScrollPosition = GUILayout.BeginScrollView(_windowScrollPosition);

			if (EditorApplication.timeSinceStartup - _refreshGUIStamp > _refreshGUIDelay) {
				_sessionTimeString = $"Время сессии: {ProjectTimeData.GetTimeString((int)EditorApplication.timeSinceStartup)}";
				_totalTimeString = $"Всего: {ProjectTimeData.GetTimeString((int)(_totalTimeStamp + EditorApplication.timeSinceStartup - _timeRefreshStamp))}";
				if (_showMonthByDaySessions) {
					_last30DaysTimeString = GetLast30DaysSessionsString();
				}
				_refreshGUIStamp = EditorApplication.timeSinceStartup;
			}

			GUILayout.Box(_timeIcon, GUILayout.Height(50), GUILayout.ExpandWidth(true));
			GUILayout.Label(_sessionTimeString); 
			GUILayout.Label(_totalTimeString); 

            if (_showMonthByDaySessions == false && GUILayout.Button("Показать сессии последние 30 дней")) {
				_last30DaysTimeString = GetLast30DaysSessionsString();
				_showMonthByDaySessions = true;
			}
			else if (_showMonthByDaySessions && GUILayout.Button("Скрыть сессии последние 30 дней")) {
				_showMonthByDaySessions = false;
			}

            if (_showMonthByDaySessions) {
				DisplayMonthDaySessions();
			}
			GUILayout.EndScrollView();
        }

		private string GetLast30DaysSessionsString() {
			return $"Сессии за последние 30 дней:{_timeData.GetLast30DaysInfo()}";
		}

		private void DisplayMonthDaySessions() {
			EditorGUILayout.HelpBox(_last30DaysTimeString, MessageType.Info, true);
		}

		private void Init() {
			_timeIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/_Main/Scripts/Editor/Windows/ProjectTime/time_icon.png", typeof(Texture));
			this.titleContent = new GUIContent("Project time");
			_timeData = ProjectTime.Data;
			_totalTimeStamp = _timeData.TotalTimeSeconds;
			_timeRefreshStamp = ProjectTime.RefreshedFileTimeStamp;
			_refreshGUIStamp = EditorApplication.timeSinceStartup - _refreshGUIDelay - 1d;
		}
    }
}