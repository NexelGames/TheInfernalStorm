using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game {
    /// <summary>
    /// Localization manager.
    /// </summary>
    public static class LocalizationManager {
        /// <summary>
        /// Fired when localization changed.
        /// </summary>
        public static event Action OnLocalizationChanged = () => { };

        public static Dictionary<Languages, Dictionary<string, string>> Dictionary = new();
        private static Languages _language = Languages.English;

        /// <summary>
        /// Get or set language.
        /// </summary>
        public static Languages Language {
            get => _language;
            set { _language = value; OnLocalizationChanged(); }
        }

        /// <summary>
        /// Set default language.
        /// </summary>
        public static void AutoLanguage() {
            Language = Languages.English;
        }

        /// <summary>
        /// Read localization spreadsheets.
        /// </summary>
        public static void Read() {
            if (Dictionary.Count > 0) return;

            var keys = new List<string>();
            var languages = new List<Languages>();

            foreach (var sheet in LocalizationSettings.Instance.Sheets) {
                var textAsset = sheet.TextAsset;
                var lines = GetLines(textAsset.text);
                var languagesString = lines[0].Split(',').Select(i => i.Trim()).ToList();

                if (languagesString.Count != languagesString.Distinct().Count()) {
                    Debug.LogError($"Duplicated languages found in `{sheet.Name}`. This sheet is not loaded.");
                    continue;
                }

                for (var i = 1; i < languagesString.Count; i++) {
                    if (Enum.TryParse(languagesString[i], out Languages language) && !Dictionary.ContainsKey(language)) {
                        Dictionary.Add(language, new Dictionary<string, string>());
                        languages.Add(language);
                    }
                }

                for (var i = 1; i < lines.Count; i++) {
                    var columns = GetColumns(lines[i]);
                    var key = columns[0];

                    if (key == "") continue;

                    if (keys.Contains(key)) {
                        Debug.LogError($"Duplicated key `{key}` found in `{sheet.Name}`. This key is not loaded.");
                        continue;
                    }

                    keys.Add(key);

                    for (var j = 1; j <= languages.Count; j++) {
                        if (Dictionary[languages[j - 1]].ContainsKey(key)) {
                            Debug.LogError($"Duplicated key `{key}` in `{sheet.Name}`.");
                        }
                        else {
                            Dictionary[languages[j - 1]].Add(key, columns[j]);
                        }
                    }
                }
            }

            AutoLanguage();
        }

        /// <summary>
        /// Check if a key exists in localization.
        /// </summary>
        public static bool HasKey(string localizationKey) {
            return Dictionary.ContainsKey(Language) && Dictionary[Language].ContainsKey(localizationKey);
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey) {
            if (Dictionary.Count == 0) {
                Read();
            }

            if (!Dictionary.ContainsKey(Language)) throw new KeyNotFoundException("Language not found: " + Language);

            var missed = !Dictionary[Language].ContainsKey(localizationKey) || Dictionary[Language][localizationKey] == "";

            if (missed) {
                Debug.LogWarning($"Translation not found: {localizationKey} ({Language}).");

                return Dictionary[Languages.English].ContainsKey(localizationKey) ? Dictionary[Languages.English][localizationKey] : localizationKey;
            }

            return Dictionary[Language][localizationKey];
        }

        /// <summary>
        /// Get localized value by localization key.
        /// </summary>
        public static string Localize(string localizationKey, params object[] args) {
            var pattern = Localize(localizationKey);

            return string.Format(pattern, args);
        }

        public static List<string> GetLines(string text) {
            text = text.Replace("\r\n", "\n").Replace("\"\"", "[_quote_]");

            var matches = Regex.Matches(text, "\"[\\s\\S]+?\"");

            foreach (Match match in matches) {
                text = text.Replace(match.Value, match.Value.Replace("\"", null).Replace(",", "[_comma_]").Replace("\n", "[_newline_]"));
            }

            // Making uGUI line breaks to work in asian texts.
            text = text.Replace("。", "。 ").Replace("、", "、 ").Replace("：", "： ").Replace("！", "！ ").Replace("（", " （").Replace("）", "） ").Trim();

            return text.Split('\n').Where(i => i != "").ToList();
        }

        public static List<string> GetColumns(string line) {
            return line.Split(',').Select(j => j.Trim()).Select(j => j.Replace("[_quote_]", "\"").Replace("[_comma_]", ",").Replace("[_newline_]", "\n")).ToList();
        }
    }
}