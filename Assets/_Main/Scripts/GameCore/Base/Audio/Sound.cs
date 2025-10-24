using Game.Core;
using PrimeTween;
using System;
using UnityEngine;

namespace Game.Audio {
    public class Sound : PoolObject {
        [SerializeField] protected AudioSource _source;
        public bool IsPlaying => _source.isPlaying;
        public bool Loop { get => _source.loop; set => _source.loop = value; }
        public float Volume { get => _source.volume; set => _source.volume = Mathf.Clamp01(value); }
        public float Pitch { get => _source.pitch; set => _source.pitch = value; }
        /// <summary>
        /// Is sound 2d (value=true)<br>
        /// Set to false and it will be 3d</br>
        /// </summary>
        public bool Is2D { get => _source.spatialBlend < 0.01f; set => _source.spatialBlend = value ? 0f : 1f; }

        private float _soundInitVolume;

        private Tween _volumeChangeTween;
        private Tween _playbackEndCheckTween;
        private Action _onSoundPlaybackEnd;

        private const float CLIPPING_AVOID_ADD_DELAY = 0.05f;

        public void SetPosition(Vector3 worldPosition) {
            transform.position = worldPosition;
        }

        /// <summary>
        /// Add callback on sound played fully [Stop() will not execute this callback]
        /// </summary>
        public void AppendNonPersistantSoundEndCallback(Action callback) {
            _onSoundPlaybackEnd += callback;
        }

        /// <summary>
        /// Remove all callbacks on sound played fully [Stop() will not execute this callback]
        /// </summary>
        public void ClearAllSoundEndCallbacks() {
            _onSoundPlaybackEnd = null;
        }

        private void ExecuteSoundEndCallbacks() {
            _onSoundPlaybackEnd.Invoke();
            _onSoundPlaybackEnd = null;
        }

        public void Init(SoundAudioClip soundData, float pitch = 1f) {
            _source.clip = soundData.AudioClip;
            _soundInitVolume = soundData.Volume;

            if (soundData.Is2D) {
                _source.spatialBlend = 0f;
            }
            else {
                _source.spatialBlend = 1f;
                _source.dopplerLevel = soundData.DopplerLevel;
                _source.spread = soundData.Spread;
                _source.minDistance = soundData.MinDistance;
                _source.maxDistance = soundData.MaxDistance;
                _source.rolloffMode = soundData.VolumeRolloff;
            }

            _source.volume = soundData.Volume;
            _source.pitch = pitch;
            _source.outputAudioMixerGroup = soundData.MixerGroup;
        }

        public void Play() {
            _volumeChangeTween.Stop();

            if (_source.volume != _soundInitVolume) {
                _source.volume = _soundInitVolume;
            }

            _source.Play();

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                _playbackEndCheckTween.Stop();
                _playbackEndCheckTween = Tween.Delay(this, _source.clip.length / _source.pitch, target => target.ExecuteSoundEndCallbacks());
            }
        }

        public void Play(float fadeInDurationSeconds) {
            _volumeChangeTween.Stop();
            _source.volume = 0f;
            _source.Play();
            _volumeChangeTween = Tween.AudioVolume(_source, _source.volume, _soundInitVolume, fadeInDurationSeconds);

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                _playbackEndCheckTween.Stop();
                _playbackEndCheckTween = Tween.Delay(this, _source.clip.length / _source.pitch, target => target.ExecuteSoundEndCallbacks());
            }
        }

        public void PlayAndDestroy() {
            _volumeChangeTween.Stop();
            if (_source.volume != _soundInitVolume) {
                _source.volume = _soundInitVolume;
            }

            _source.Play();

            if (_onSoundPlaybackEnd != null && !_source.loop) {
                _playbackEndCheckTween.Stop();
                _playbackEndCheckTween = Tween.Delay(this, _source.clip.length / _source.pitch, target => target.ExecuteSoundEndCallbacks());
            }

            Destroy(_source.clip.length / _source.pitch + CLIPPING_AVOID_ADD_DELAY);
        }

        public void Stop() {
            if (_source.isPlaying == false) {
                return;
            }

            _volumeChangeTween.Stop();
            _playbackEndCheckTween.Stop();

            _source.Stop();
        }

        public void Stop(float fadeOutDurationSeconds, Action callback, bool stopSourceOnFadeEnd = true) {
            _volumeChangeTween.Stop();
            _playbackEndCheckTween.Stop();

            if (!_source.isPlaying) {
                callback?.Invoke();
                return;
            }

            if (stopSourceOnFadeEnd) {
                callback += _source.Stop;
            }

            _volumeChangeTween = Tween.AudioVolume(_source, _source.volume, 0f, fadeOutDurationSeconds)
                .OnComplete(callback);
        }

        public override void OnObjectReuse() {
            base.OnObjectReuse();

            _volumeChangeTween.Stop();
            _playbackEndCheckTween.Stop();

            if (_source.isPlaying) {
                _source.Stop();
            }
            Loop = false;
        }
    }
}
