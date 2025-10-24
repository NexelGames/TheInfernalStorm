using Game.Core;
using UnityEngine;

namespace Game.Audio {
    [System.Serializable]
    public class RepeatSound2D {
        [SerializeField] private CompoundSound _compoundSound;
        [Space]
        [SerializeField] private float _repeatDelay;
        [Tooltip("for example, 0.2 random will add to repeat delay value from 0 to 0.2 randomly")]
        [SerializeField] private float _repeatDelayRandom;
        [Tooltip("leave -1 to play till script call Stop(). 0 will play once, 1 two times and etc")]
        [SerializeField, Min(-1)] private int _repeatTimes = -1;
        [Tooltip("Slow-motion will not affect on repeat delay")]
        [SerializeField] private bool _repeatDelayUnscaled;

        private MonoBehaviour _linkedMono;
        private Coroutine _repeatSoundCoroutine;
        private int _repeatCounter = -1;

        public void Init(MonoBehaviour monoBehaviour) {
            _linkedMono = monoBehaviour;
            _compoundSound.Init(monoBehaviour);
        }

        /// <summary>
        /// Resets repeat counter and plays repeat sound like first time
        /// </summary>
        public void Play() {
            _repeatCounter = -1; // reset
            Repeat();
        }

        private void Repeat() {
            if (_repeatTimes != -1) {
                if (_repeatCounter < _repeatTimes) {
                    _repeatCounter++;
                }
                else {
                    return;
                }
            }

            _compoundSound.Play();

            if (_repeatDelayRandom > 0) {
                _repeatSoundCoroutine = CoroutineHelper.WaitForAction(_linkedMono, Repeat, _repeatDelay + Random.Range(0f, _repeatDelayRandom), _repeatDelayUnscaled);
            }
            else {
                _repeatSoundCoroutine = CoroutineHelper.WaitForAction(_linkedMono, Repeat, _repeatDelay, _repeatDelayUnscaled);
            }
        }

        public void Stop() {
            if (_repeatSoundCoroutine != null) _linkedMono.StopCoroutine(_repeatSoundCoroutine);
            _compoundSound.Stop();
        }
    }
}