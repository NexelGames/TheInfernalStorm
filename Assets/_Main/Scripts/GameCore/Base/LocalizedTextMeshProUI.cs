using TMPro;
using UnityEngine;

namespace Game.UI {
    public class LocalizedTextMeshProUI : TextMeshProUGUI {
        public string LocalizedKey {
            get => _localizedKey;
            set {
                _localizedKey = value;
                Localize();
            }
        }

        [Header("Localization")]
        [SerializeField] private string _localizedKey;

        protected override void OnEnable() {
            base.OnEnable();
            Localize();
            LocalizationManager.OnLocalizationChanged += Localize;
        }

        protected override void OnDestroy() {
            LocalizationManager.OnLocalizationChanged -= Localize;
            base.OnDestroy();
        }

        private void Localize() {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif
            base.text = LocalizationManager.Localize(_localizedKey);
        }
    }
}