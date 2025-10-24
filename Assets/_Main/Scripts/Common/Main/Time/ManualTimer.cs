using System;
using UnityEngine;

namespace Game {
    [Serializable]
    public struct ManualTimer {
        public bool IsElapsed => _delay < _timer;

        /// <summary>
        /// Timer goal in seconds
        /// </summary>
        public float Delay {
            get => _delay;
            set => _delay = value;
        }

        /// <summary>
        /// Passed seconds
        /// </summary>
        public float Timer {
            get => _timer;
            set => _timer = value;
        }

        [SerializeField] private float _delay;
        private float _timer;

        public ManualTimer(float delaySeconds) {
            _timer = 0f;
            _delay = delaySeconds;
        }
        public ManualTimer(float timerValueSeconds, float delay) {
            _timer = timerValueSeconds;
            _delay = delay;
        }

        public void Increment(float seconds) {
            _timer += seconds;
        }

        public void NullTimer() {
            _timer = 0f;
        }

        public void Complete() {
            _timer = _delay + 1f;
        }
    }
}