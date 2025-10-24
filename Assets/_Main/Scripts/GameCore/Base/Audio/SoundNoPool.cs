using UnityEngine;

namespace Game.Audio {
    /// <summary>
    /// Sound that can be used in scene (not from pool)
    /// </summary>
    public class SoundNoPool : Sound {
        [SerializeField] private SoundAudioClip _soundData;

        private void Awake() {
            Init(_soundData, _source.pitch);
        }
    }
}
