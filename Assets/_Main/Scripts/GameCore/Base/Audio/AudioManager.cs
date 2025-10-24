using Game.Core;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio {
    public class AudioManager : MonoBehaviour {
        #region SINGLETON PATTERN
        private static AudioManager _instance;
        public static bool IsCreated => _instance != null;
        public static AudioManager Instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);

                    if (_instance == null) {
                        GameObject container = new GameObject("AudioManager");
                        _instance = container.AddComponent<AudioManager>();
                        _instance.GetSettings();
                    }
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// All game sounds on/off
        /// </summary>
        public bool SoundsEnabled {
            set {
                _audioSettings.SoundsEnabled = value;
                if (value) {
                    _masterMixer.SetFloat(MasterVolumeRefName, _defaultMasterVolume);
                }
                else {
                    _masterMixer.SetFloat(MasterVolumeRefName, OffVolumeLevel);
                }
                _soundsEnabled = value;
            }
            get => _soundsEnabled;
        }
        /// <summary>
        /// Game sound effects (steps, environment) level
        /// </summary>
        public float SoundLevel {
            set {
                value = Mathf.Clamp01(value);
                _audioSettings.SoundLevelNormalized = value;
                if (value > 0) {
                    _masterMixer.SetFloat(SoundsVolumeRefName, Mathf.Lerp(LowestOnVolumeLevel, _defaultMasterVolume, value));
                }
                else {
                    _masterMixer.SetFloat(SoundsVolumeRefName, OffVolumeLevel);
                }
                _soundsLevel = value;
            }
            get => _soundsLevel;
        }
        /// <summary>
        /// Game music level
        /// </summary>
        public float MusicLevel {
            set {
                value = Mathf.Clamp01(value);
                _audioSettings.MusicLevelNormalized = value;
                if (value > 0) {
                    _masterMixer.SetFloat(MusicVolumeRefName, Mathf.Lerp(LowestOnVolumeLevel, _defaultMasterVolume, value));
                }
                else {
                    _masterMixer.SetFloat(MusicVolumeRefName, OffVolumeLevel);
                }
                _musicLevel = value;
            }
            get => _musicLevel;
        }

        /// <summary>
        /// Is main music playing
        /// </summary>
        public bool IsMusicPlaying => _mainMusicSoundObj.IsPlaying;

        [SerializeField] private AudioMixer _masterMixer;
        [Space(10f), SerializeField] private Sound _mainMusicSoundObj;
        [SerializeField] private float _mainMusicFadeInOutDuration = 1f;

        private AudioSettings _audioSettings;

        private readonly string MasterVolumeRefName = "MasterVolume";
        private readonly string SoundsVolumeRefName = "SoundsVolume";
        private readonly string MusicVolumeRefName = "MusicVolume";
        private readonly float LowestOnVolumeLevel = -40f;
        private readonly float OffVolumeLevel = -80f;

        private bool _soundsEnabled;
        private float _musicLevel;
        private float _soundsLevel;
        private float _defaultMasterVolume;

        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _masterMixer.GetFloat(MasterVolumeRefName, out _defaultMasterVolume);

#if UNITY_EDITOR
            if (transform.parent != null) transform.parent = null;
#endif
            GetSettings();
            DontDestroyOnLoad(gameObject);
        }

        private void GetSettings() {
            _audioSettings ??= SaveSystem.Instance.GameSettings.AudioSettings;
        }

        private void Start() {
            MusicLevel = _audioSettings.MusicLevelNormalized;
            SoundLevel = _audioSettings.SoundLevelNormalized;
            SoundsEnabled = _audioSettings.SoundsEnabled;

            SubscribeToButtonSoundClick();
        }

        internal void SubscribeToButtonSoundClick() {
            UI.VibroBtn.OnSoundClick += OnButtonSoundClick;
        }

        private void OnButtonSoundClick(SoundAudioClip audioData) {
            PlaySingleShot(audioData);
        }

        /// <summary>
        /// Play once
        /// </summary>
        public void PlaySingleShot(SoundAudioClip audioData, float pitch = 1f) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            Sound sound = PoolManager.instance.ReuseSound();
            sound.Init(audioData, pitch);
            sound.PlayAndDestroy();
        }

        /// <summary>
        /// Play once with callback, not that <see cref="SoundAudioClip.hasSoundEndCallback"/> must be true <br>
        /// If false - no callbacks will be</br>
        /// </summary>
        public void PlaySingleShot(SoundAudioClip audioData, Action onPlayed, float pitch = 1f) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            Sound sound = PoolManager.instance.ReuseSound();
            sound.Init(audioData, pitch);
            sound.AppendNonPersistantSoundEndCallback(onPlayed);
            sound.PlayAndDestroy();
        }

        /// <summary>
        /// Play at position once <br>
        /// For 2d sounds use <see cref="PlaySingleShot(SoundAudioClip, float)"/></br>
        /// </summary>
        /// <param name="audioData"></param>
        /// <param name="worldPoint"></param>
        /// <param name="pitch"></param>
        public void PlaySingleShotAt(SoundAudioClip audioData, Vector3 worldPoint, float pitch = 1f) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            Sound sound = PoolManager.instance.ReuseSound();
            sound.Init(audioData, pitch);
            sound.SetPosition(worldPoint);
            sound.PlayAndDestroy();
        }

        /// <summary>
        /// Play at position once with callback, not that <see cref="SoundAudioClip.hasSoundEndCallback"/> must be true <br>
        /// If false - no callbacks will be</br>
        /// </summary>
        public void PlaySingleShotAt(SoundAudioClip audioData, Vector3 worldPoint, Action onPlayed, float pitch = 1f) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            Sound sound = PoolManager.instance.ReuseSound();
            sound.Init(audioData, pitch);
            sound.SetPosition(worldPoint);
            sound.AppendNonPersistantSoundEndCallback(onPlayed);
            sound.PlayAndDestroy();
        }

        /// <summary>
        /// The most efficient way to play sound<br>
        /// You need to cache <see cref="Sound"/> obj and then put it here</br><br>
        /// You can change soundObj properties in other scripts</br><br>
        /// This method will only check if <see cref="_soundsEnabled"/> before Play</br>
        /// </summary>
        /// <param name="soundObj"></param>
        public void PlaySingleShot(Sound soundObj) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            soundObj.Play();
        }

        /// <summary>
        /// The most efficient way to play sound<br>
        /// You need to cache <see cref="Sound"/> obj and then put it here</br><br>
        /// You can change soundObj properties in other scripts</br><br>
        /// This method will only check if <see cref="_soundsEnabled"/> before Play</br>
        /// </summary>
        /// <param name="soundObj"></param>
        /// <param name="fadeInDuration">duration of sound fade in (smooth appear)</param>
        public void PlaySingleShot(Sound soundObj, float fadeInDuration = 0.5f) {
            if (!_soundsEnabled || _soundsLevel == 0f) {
                return;
            }

            soundObj.Play(fadeInDuration);
        }

        public void ChangeMusic(SoundAudioClip toMusic, float fadeInOutDuration = -1f, Action onPlayedCallback = null) {
            if (fadeInOutDuration < 0f) {
                fadeInOutDuration = _mainMusicFadeInOutDuration;
            }

            void PlayNewMusic() {
                _mainMusicSoundObj.Init(toMusic, 1f);
                if (onPlayedCallback != null) {
                    _mainMusicSoundObj.AppendNonPersistantSoundEndCallback(onPlayedCallback);
                }
                _mainMusicSoundObj.Play(fadeInOutDuration);
            }

            if (_mainMusicSoundObj.IsPlaying == false) {
                PlayNewMusic();
                return;
            }

            _mainMusicSoundObj.Stop(fadeInOutDuration, PlayNewMusic, stopSourceOnFadeEnd: false);
        }
    }
}