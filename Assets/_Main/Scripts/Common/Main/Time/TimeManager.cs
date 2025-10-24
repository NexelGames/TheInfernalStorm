using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {

    /// <summary>
    /// Main class to handle game time
    /// </summary>
    public class TimeManager : MonoSingleton<TimeManager> {
        public static bool IsExist => _instance != null;

        public static UnityEvent OnRealSecondTick = new();
        public static UnityEvent<TimeEvent> OnTimeFreezeChanged = new();

        private float _lastTimeScale;
        private float _defaultTimeScale;
        public float DefaultTimeScale => _defaultTimeScale;

        private float _defaultFixedDeltaTime;
        public float DefaultFixedDeltaTime => _defaultFixedDeltaTime;

        private Coroutine _motionCoroutine = null;
        private TimeEvent _timeEventData = new();

        // used only for global tick coroutine, dont reuse in other!
        private WaitForSecondsRealtime _realTimeSecond = new(1f);

        protected override void Awake() {
            base.Awake();
            if (_awakeDestroyObj) {
                return;
            }
            _defaultTimeScale = Time.timeScale;
            _defaultFixedDeltaTime = Time.fixedDeltaTime;

            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            if (_awakeDestroyObj) {
                return;
            }

            StartCoroutine(GlobalRealTimeTick());
        }

        public static DateTime UnixTimeStampMillisecondsToUTCDateTime(double unixTimeStamp) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)unixTimeStamp);
            return dateTimeOffset.UtcDateTime;
        }

        public static DateTime UnixTimeStampSecondsToUTCDateTime(double unixTimeStamp) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp);
            return dateTimeOffset.UtcDateTime;
        }

        public static string FormatTime00x00(int seconds) {
            if (seconds >= 3600) {
                int hours = seconds / 3600;
                int minutes = (seconds / 60) % 60;
                return $"{hours:00}:{minutes:00}";
            }
            else if (seconds >= 60) {
                int minutes = seconds / 60;
                int second = seconds % 60;
                return $"{minutes:00}:{second:00}";
            }
            else {
                return $"00:{seconds:00}";
            }
        }
		
		public static string FormatTimeHours(int seconds)
        {
            int hours = seconds / 3600;

            if (hours > 24) return $"{hours / 24}d {hours % 24}h";
            else return $"{hours}h {seconds % 3600 / 60}m";
        }

        /// <summary>
        /// Resets time settings to default
        /// <code>
        ///    Time.timeScale = defaultTimeScale;
        ///    Time.fixedDeltaTime = defaultFixedDeltaTime;
        /// </code>
        /// </summary>
        public void ResetTimeToDefault() {
            _lastTimeScale = Time.timeScale;

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            Time.timeScale = _defaultTimeScale;
            Time.fixedDeltaTime = _defaultFixedDeltaTime;

            if (_lastTimeScale == 0f) {
                SendFreezeChangeEvent(false);
            }
        }

        /// <summary>
        /// Freezes time by scale
        /// </summary>
        public void FreezeTime() {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Time is already freezed and you tried to freeze it!", this);
#endif
                return;
            }

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            _lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            SendFreezeChangeEvent(true);
        }

        /// <summary>
        /// Unfreezes time by scale
        /// </summary>
        /// <param name="useDefault">Unfreeze and keep default timeScale
        /// <code>
        ///     Time.timeScale = defaultTimeScale;
        /// </code>
        /// </param>
        public void UnfreezeTime(bool useDefault = false) {
            if (Time.timeScale != 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Time was not freezed, but you tried to unfreeze it!", this);
#endif
                return;
            }

            if (useDefault) {
                ResetTimeToDefault();
            }
            else {
                Time.timeScale = _lastTimeScale;
                SendFreezeChangeEvent(false);
            }
        }

        public void SetTimeScale(float scale) {
            if (Time.timeScale > 0f && scale == 0f) {
                FreezeTime();
                return;
            }

            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            _lastTimeScale = Time.timeScale;
            Time.timeScale = scale;

            if (Time.timeScale == 0f && scale > 0f) {
                _timeEventData.IsFreezed = false;
                OnTimeFreezeChanged.Invoke(_timeEventData);
            }
        }

        private void SendFreezeChangeEvent(bool isFreezed) {
            _timeEventData.IsFreezed = isFreezed;
            OnTimeFreezeChanged.Invoke(_timeEventData);
        }

        #region Coroutine Timers
        public Coroutine RunTimerInt(float forSeconds, bool unscaled, Action<float> tickCallback) {
            return StartCoroutine(TimerRunInt(forSeconds, unscaled, tickCallback));
        }

        public Coroutine RunTimerInt(MonoBehaviour behaviour, float forSeconds, bool unscaled, Action<float> tickCallback) {
            return behaviour.StartCoroutine(TimerRunInt(forSeconds, unscaled, tickCallback));
        }

        public void StopTimer(ref Coroutine coroutine) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private IEnumerator TimerRunInt(float seconds, bool unscaled, Action<float> tickCallback) {
            float firstDelay = seconds % 1;
            while (firstDelay > 0) {
                yield return null;
                if (unscaled) firstDelay -= Time.unscaledDeltaTime;
                else firstDelay -= Time.deltaTime;
            }

            if (seconds % 1 > 0) {
                seconds -= (seconds % 1);
            }
            tickCallback.Invoke(seconds);

            if (unscaled) {
                var second = new WaitForSecondsRealtime(1f);
                while (seconds > 0) {
                    yield return second;
                    seconds--;
                    tickCallback.Invoke(seconds);
                }
            }
            else {
                var second = new WaitForSeconds(1f);
                while (seconds > 0) {
                    yield return second;
                    seconds--;
                    tickCallback.Invoke(seconds);
                }
            }
        }

        private IEnumerator GlobalRealTimeTick() {
            while (true) {
                yield return _realTimeSecond;
                OnRealSecondTick.Invoke();
            }
        }
        #endregion

        #region Time Motion Effect
        private TimeMotionEffectData _timeMotionData;
        private UnityAction _onTimeMotionEffectComplete;
        public void DoSlowMotion(TimeMotionEffectData data, UnityAction onEndCallback = null) {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Can't do time motion effect, time is freezed!", this);
#endif
                return;
            }

            if (_motionCoroutine != null) {
                StopCoroutine(_motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            //cache slow motion data
            _timeMotionData = data;
            _motionCoroutine = StartCoroutine(SlowMotion(onEndCallback));
        }

        public void DoSpeedUpMotion(TimeMotionEffectData data, UnityAction onEndCallback = null) {
            if (Time.timeScale == 0f) {
#if UNITY_EDITOR
                Debug.LogWarning("Can't do time speed up motion effect, time is freezed!", this);
#endif
                return;
            }

            if (_motionCoroutine != null) {
                StopCoroutine(_motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }

            //cache slow motion data
            _timeMotionData = data;
            _motionCoroutine = StartCoroutine(SpeedUpMotion(onEndCallback));
        }

        public void StopTimeMotionEffect() {
            if (_motionCoroutine != null) {
                CoroutineHelper.Stop(this, ref _motionCoroutine);
                _onTimeMotionEffectComplete?.Invoke();
            }
        }

        IEnumerator SlowMotion(UnityAction onEndCallback) {
            float interpolationFactor;
            float startTimeScale = Time.timeScale;
            float startFixedDeltaTime = Time.fixedDeltaTime;
            WaitForSecondsRealtime realTimeFixedDelay = new WaitForSecondsRealtime(_defaultFixedDeltaTime);

            _onTimeMotionEffectComplete = () => {
                Time.timeScale = startTimeScale;
                Time.fixedDeltaTime = startFixedDeltaTime;
            };
            if (onEndCallback != null) _onTimeMotionEffectComplete += onEndCallback;

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeIn) {
                interpolationFactor = _timeMotionData.CurveIn.Evaluate(i);
                Time.timeScale = Mathf.Lerp(startTimeScale, _timeMotionData.Scale, interpolationFactor);
                Time.fixedDeltaTime = Mathf.Lerp(startFixedDeltaTime, _timeMotionData.Scale * startFixedDeltaTime, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            Time.timeScale = _timeMotionData.Scale;
            Time.fixedDeltaTime = _timeMotionData.Scale * startFixedDeltaTime;

            if (_timeMotionData.Duration < 0f) {
                onEndCallback?.Invoke();
                _onTimeMotionEffectComplete = null;
                yield break;
            }
            yield return new WaitForSecondsRealtime(_timeMotionData.Duration);

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeOut) {
                interpolationFactor = _timeMotionData.CurveOut.Evaluate(i);
                Time.timeScale = Mathf.Lerp(_timeMotionData.Scale, startTimeScale, interpolationFactor);
                Time.fixedDeltaTime = Mathf.Lerp(_timeMotionData.Scale * startFixedDeltaTime, startFixedDeltaTime, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            _motionCoroutine = null;
            _onTimeMotionEffectComplete.Invoke();
        }

        IEnumerator SpeedUpMotion(UnityAction onEndCallback) {
            float interpolationFactor;
            float startTimeScale = Time.timeScale;
            WaitForSecondsRealtime realTimeFixedDelay = new WaitForSecondsRealtime(_defaultFixedDeltaTime);

            _onTimeMotionEffectComplete = () => {
                Time.timeScale = startTimeScale;
            };
            if (onEndCallback != null) _onTimeMotionEffectComplete += onEndCallback;

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeIn) {
                interpolationFactor = _timeMotionData.CurveIn.Evaluate(i);
                Time.timeScale = Mathf.Lerp(startTimeScale, _timeMotionData.Scale, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            Time.timeScale = _timeMotionData.Scale;

            if (_timeMotionData.Duration < 0f) {
                onEndCallback?.Invoke();
                _onTimeMotionEffectComplete = null;
                yield break;
            }
            yield return new WaitForSecondsRealtime(_timeMotionData.Duration);

            for (float i = 0; i < 1f; i += _defaultFixedDeltaTime / _timeMotionData.TimeOut) {
                interpolationFactor = _timeMotionData.CurveOut.Evaluate(i);
                Time.timeScale = Mathf.Lerp(_timeMotionData.Scale, startTimeScale, interpolationFactor);

                yield return realTimeFixedDelay;
            }

            _motionCoroutine = null;
            _onTimeMotionEffectComplete.Invoke();
        }
        #endregion
    }
}