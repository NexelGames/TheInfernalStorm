using Game.JoystickUI;
using UnityEngine;

namespace Game {
    [System.Serializable]
    public class GameSettings {
        public AudioSettings AudioSettings = new();
        public GraphicsSettings GraphicsSettings = new();
        public ControlsSettings ControlsSettings = new();
    }

    [System.Serializable]
    public class AudioSettings {
        /// <summary>
        /// Game sounds on/off, to manipulate use <see cref="Audio.AudioManager"/> API
        /// </summary>
        public bool SoundsEnabled = true;
        public float SoundLevelNormalized = 1f;
        public float MusicLevelNormalized = 1f;
    }

    [System.Serializable]
    public class GraphicsSettings {

    }

    [System.Serializable]
    public class ControlsSettings {
        /// <summary>
        /// Game haptic on/off, to manipulate use <see cref="Core.VibrationsManager"/> API
        /// </summary>
        public bool HapticEnabled = true;
    }
}