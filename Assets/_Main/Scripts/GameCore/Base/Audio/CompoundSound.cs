using Game.Core;
using UnityEngine;

namespace Game.Audio {
    [System.Serializable]
    public class CompoundSound {
        [Tooltip("Means sounds will be taken from pool and don't get back. This can increase pool size, but reuse performance the best")]
        [SerializeField] private bool _keepSoundObjectsReference;
        [Tooltip("Means queue sounds with delay running will not be played on new Play() call")]
        [SerializeField] private bool _stopPreviousPlayCallOnPlay;
        [SerializeField] private CompoundSoundData[] _sounds;

        private Sound[] _reuseSounds;
        private MonoBehaviour _linkedMono;
        private Coroutine[] _soundsInvokeDelays;

        public void Init(MonoBehaviour monoBehaviour) {
            _linkedMono = monoBehaviour;
            if (_keepSoundObjectsReference) {
                _reuseSounds = new Sound[_sounds.Length];
                for (int i = 0; i < _reuseSounds.Length; i++) {
                    _reuseSounds[i] = PoolManager.instance.ReuseSound();
                    _reuseSounds[i].Init(_sounds[i].Clip);
                }
            }
            _soundsInvokeDelays = new Coroutine[_sounds.Length];
        }

        public void Play() {
            if (_stopPreviousPlayCallOnPlay) Stop();
            for (int i = 0; i < _sounds.Length; i++) {
                if (_sounds[i].PlayDelay > 0) {
                    int index = i;
                    _soundsInvokeDelays[i] = CoroutineHelper.WaitForAction(_linkedMono, 
                        () => PlaySound(index), _sounds[i].PlayDelay, _sounds[i].PlayDelayUnscaled);
                }
                else {
                    PlaySound(i);
                }
            }
        }

        public void Stop() {
            for (int i = 0; i < _soundsInvokeDelays.Length; i++) {
                if (_soundsInvokeDelays[i] != null) {
                    _linkedMono.StopCoroutine(_soundsInvokeDelays[i]);
                }
            }
        }

        private void PlaySound(int index) {
            float randomPitch = _sounds[index].RandomPitch;
            if (_keepSoundObjectsReference) {
                if (randomPitch > 0) {
                    _reuseSounds[index].Pitch = Random.Range(1f - randomPitch, 1f + randomPitch);
                }
                AudioManager.Instance.PlaySingleShot(_reuseSounds[index]);
            }
            else {
                if (randomPitch > 0) {
                    randomPitch = Random.Range(1f - randomPitch, 1f + randomPitch);
                }
                else {
                    randomPitch = 1f;
                }
                AudioManager.Instance.PlaySingleShot(_sounds[index].Clip, randomPitch);
            }
        }
    }

    [System.Serializable]
    public class CompoundSoundData {
        public SoundAudioClip Clip;
        
        [Space]
        [Tooltip("random pitch around 1 value. if you set 0.2 it will make pitch 0.8f-1.2f.\n0 value will not change pitch at all")]
        public float RandomPitch;
        [Tooltip("should sound play with delay after call")]
        public float PlayDelay;
        [Tooltip("Unscaled means slow-motion will not affect to delay if set true. \nDo nothing if Play Delay is 0")]
        public bool PlayDelayUnscaled;
    }
}