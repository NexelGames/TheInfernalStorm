using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    /// <summary>
    /// Asset usage example.
    /// </summary>
    public class Example : MonoBehaviour {
        public Text FormattedText;

        [Space]
        public Button GermanSetButton;
        public Button RussianSetButton;
        public Button EnglishSetButton;

        /// <summary>
        /// Called on app start.
        /// </summary>
        public void Awake() {
            LocalizationManager.Read();

            GermanSetButton.onClick.AddListener(() => SetLocalization(Languages.German));
            RussianSetButton.onClick.AddListener(() => SetLocalization(Languages.Russian));
            EnglishSetButton.onClick.AddListener(() => SetLocalization(Languages.English));

            switch (Application.systemLanguage) {
                case SystemLanguage.German:
                    LocalizationManager.Language = Languages.German;
                    break;
                case SystemLanguage.Russian:
                    LocalizationManager.Language = Languages.Russian;
                    break;
                default:
                    LocalizationManager.Language = Languages.English;
                    break;
            }

            // This way you can localize and format strings from code.
            FormattedText.text = LocalizationManager.Localize("Settings.Example.PlayTime", TimeSpan.FromHours(10.5f).TotalHours);

            // This way you can subscribe to LocalizationChanged event.
            LocalizationManager.OnLocalizationChanged += () => FormattedText.text = LocalizationManager.Localize("Settings.Example.PlayTime", TimeSpan.FromHours(10.5f).TotalHours);
        }

        /// <summary>
        /// Change localization at runtime.
        /// </summary>
        public void SetLocalization(Languages language) {
            LocalizationManager.Language = language;
        }

        /// <summary>
        /// Write a review.
        /// </summary>
        public void Review() {
            Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/120113");
        }
    }
}