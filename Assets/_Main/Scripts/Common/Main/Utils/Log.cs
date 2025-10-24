using System;
using UnityEngine;

namespace Game.CustomLog {
    public enum CustomLogMessageType : byte {
        System = 0,
        Debug = 1,
        Game = 2,
        Error = 3
    }

    public class Log : MonoBehaviour, ILogHandler {

        #region Singleton

        private static Log _instance;
        private static Log Instance {
            get {
                if (_instance == null) {
                    GameObject go = new GameObject(typeof(Log).ToString());
                    _instance = go.AddComponent<Log>();
                }
                return _instance;
            }
        }

        protected void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _logger = new Logger(this);
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        private static Logger _logger;
        public static Logger Logger => _logger;

        /// <summary>
        /// Push message
        /// </summary>
        /// <param name="type">Log type. Each type has it's own color</param>
        /// <param name="context">Object that belongs to this message (you can select obj by clicking on message)</param>
        public static void Push(string text, CustomLogMessageType type, UnityEngine.Object context = null) {
            if (text == string.Empty) return;

            Instance.PushLogInstance(text, type, context);
        }

        /// <summary>
        /// Push Debug message
        /// </summary>
        /// <param name="context">Object that belongs to this message (you can select obj by clicking on message)</param>
        public static void Push(string text, UnityEngine.Object context = null) {
            if (text == string.Empty) return;
            Instance.PushLogInstance(text, context);
        }

        private static string _system_color = "#73b2ff";
        private static string _debug_color = "#ffe51f";
        private static string _game_color = "#75ff78";
        private static string _error_color = "#ff0f37";

        public const string WHITE_COLOR = "#ffffff";
        public const string GRAY_COLOR = "#dedede";

        private void PushLogInstance(string text, UnityEngine.Object context = null) {
            _logger.Log(LogType.Log, $"<color={_debug_color}>[{CustomLogMessageType.Debug}]</color>",
                      $"<color={GetColorMessage(CustomLogMessageType.Debug)}>{text}</color>", context);
        }

        private void PushLogInstance(string text, CustomLogMessageType type, UnityEngine.Object context = null) {
            _logger.Log(GetLogType(type), $"<color={GetColorSystem(type)}>[{type}]</color>",
              $"<color={GetColorMessage(type)}>{text}</color>", context);
        }

        private static string GetColorSystem(CustomLogMessageType type) {
            switch (type) {
                case CustomLogMessageType.System: return _system_color;
                case CustomLogMessageType.Debug: return _debug_color;
                case CustomLogMessageType.Game: return _game_color;
                case CustomLogMessageType.Error: return _error_color;
            }
            return WHITE_COLOR;
        }

        private static LogType GetLogType(CustomLogMessageType messageType) {
            switch (messageType) {
                case CustomLogMessageType.System: return LogType.Warning;
                case CustomLogMessageType.Debug: return LogType.Log;
                case CustomLogMessageType.Game: return LogType.Log;
                case CustomLogMessageType.Error: return LogType.Error;
            }
            return LogType.Log;
        }

        private static string GetColorMessage(CustomLogMessageType type) {
            switch (type) {
                case CustomLogMessageType.System: return GRAY_COLOR;
                case CustomLogMessageType.Debug: return GRAY_COLOR;
                case CustomLogMessageType.Game: return GRAY_COLOR;
                case CustomLogMessageType.Error: return GRAY_COLOR;
            }
            return WHITE_COLOR;
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
            Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context) {
            Debug.unityLogger.LogException(exception, context);
        }
    }
}