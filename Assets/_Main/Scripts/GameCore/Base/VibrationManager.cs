using Game.UI;
using Lofelt.NiceVibrations;

namespace Game.Core {

    /// <summary>
    /// Vibration manager x NiceVibrations (https://feel-docs.moremountains.com/nice-vibrations.html)
    /// </summary>
    public class VibrationsManager {
        /// <summary>
        /// This is cooldown between vibrations call, 0.2 seconds by default<br>
        /// Any vibration call before cooldown elapsed will be ignored</br>
        /// </summary>
        private CooldownUnscaled _hapticCooldown = new(0.2f);

        public VibrationsManager() {
            _controlsSettings = SaveSystem.Instance.GameSettings.ControlsSettings;
            _hapticEnabled = _controlsSettings.HapticEnabled;
            VibroBtn.OnClick += (vibroType) => {
                if (vibroType != HapticPatterns.PresetType.None) DoVibro(vibroType);
            };
            HapticController.Init();
        }

        #region SINGLETON PATTERN
        private static VibrationsManager _instance;
        public static VibrationsManager Instance {
            get {
                if (_instance == null) {
                    _instance = new VibrationsManager();
                }
                return _instance;
            }
        }
        public static void Create() => _instance = Instance;
        #endregion

        private ControlsSettings _controlsSettings;
        private bool _hapticEnabled;
        /// <summary>
        /// Is user settings vibro enabled
        /// </summary>
        public bool HapticEnabled {
            get => _hapticEnabled;
            set {
                _hapticEnabled = value;
                _controlsSettings.HapticEnabled = _hapticEnabled;
            }
        }

        /// <summary>
        /// Triggers vibration with <paramref name="hapticType"/> pattern
        /// </summary>
        public void DoVibro(HapticPatterns.PresetType hapticType) {
            if (_hapticEnabled && _hapticCooldown.IsElapsed) {
                HapticPatterns.PlayPreset(hapticType);
                _hapticCooldown.Reset();
            }
        }
    }
}