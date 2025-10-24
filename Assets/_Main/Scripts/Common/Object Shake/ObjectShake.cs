using UnityEngine;

namespace Game.Core {
    [System.Serializable]
    public struct ShakeSnapshoot {
        [Min(0f)] public float ShakeAmount;
        [Min(0f)] public float Frequency;
        public bool TimeDependent;
        [Min(0f)] public float ShakeTimeIn;
        [Min(0f)] public float ShakeTimeDecay;
        [Min(0f)] public float ShakeTimeOut;
        public bool OverrideShakeIfRunning;
        [HideInInspector] public float Seed;
    }

    /// <summary>
    /// Enhanced object shake <br>
    /// Use with public shake functions only </br>
    /// </summary>
	[AddComponentMenu("Common/Shake Effect/Object Shake")]
    public class ObjectShake : MonoBehaviour {
        public Transform ObjShakeTransform;

        public bool IsContiniusShakeOn => _continiousShakeOn;

        private Vector3 _originalPos;

        [SerializeField] private AnimationCurve _shakeInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _shakeOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float _currentShakeAmount = 0f;
        private float _timerIn;
        private float _timerOut;
        private float _timerDecay;

        private bool _ignoreShake = false;
        /// <summary>
        /// Set it to true to ignore DoShake() Calls
        /// </summary>
        public bool IgnoreShake {
            get => _ignoreShake;
            set {
                if (value && enabled) {
                    // if ignore activated but shake was in progress
                    ObjShakeTransform.localPosition = _originalPos;
                    enabled = false;
                }
                _ignoreShake = value;
            }
        }

        private float _shakeAmount;
        private float _frequency;
        private bool _timeDependent;
        private float _shakeTimeIn;
        private float _shakeTimeOut;
        private float _shakeTimeDecay;
        private float _seed;
        private bool _continiousShakeOn = false;

        private Vector3 _perlinNoise;
        private ShakeSnapshoot _shakeSnapshoot;

        void Awake() {
            if (ObjShakeTransform == null) {
                ObjShakeTransform = transform;
            }

            if (enabled) {
                enabled = false;
            }
        }

        void LateUpdate() {
            float deltaTime;
            if (_timeDependent) {
                deltaTime = Time.deltaTime;
                _perlinNoise = new Vector3(Mathf.PerlinNoise(_seed, Time.time * _frequency) * 2 - 1,
                                           Mathf.PerlinNoise(_seed + 1f, Time.time * _frequency) * 2 - 1,
                                           Mathf.PerlinNoise(_seed + 2f, Time.time * _frequency) * 2 - 1);
            }
            else {
                deltaTime = Time.unscaledDeltaTime;
                _perlinNoise = new Vector3(Mathf.PerlinNoise(_seed, Time.realtimeSinceStartup * _frequency) * 2 - 1,
                                           Mathf.PerlinNoise(_seed + 1f, Time.realtimeSinceStartup * _frequency) * 2 - 1,
                                           Mathf.PerlinNoise(_seed + 2f, Time.realtimeSinceStartup * _frequency) * 2 - 1);
            }

            if (_timerIn <= _shakeTimeIn) {
                _currentShakeAmount = Mathf.Lerp(0f, _shakeAmount, _shakeInCurve.Evaluate(_timerIn / _shakeTimeIn));
                _timerIn += deltaTime;

                if (_timerIn >= _shakeTimeIn) {
                    _timerIn += 0.1f;
                    _currentShakeAmount = _shakeAmount;
                }
            }
            else if (_timerDecay < _shakeTimeDecay) {
                _timerDecay += deltaTime;
            }
            else if (_timerOut <= _shakeTimeOut) {
                _currentShakeAmount = Mathf.Lerp(_shakeAmount, 0f, _shakeOutCurve.Evaluate(_timerOut / _shakeTimeOut));
                _timerOut += deltaTime;

                if (_timerOut >= _shakeTimeOut) {
                    _timerOut += 0.1f;

                    if (_continiousShakeOn) {
                        DoContiniousShake(_shakeSnapshoot);
                    }
                    else {
                        StopShake();
                    }

                    return;
                }
            }

            ObjShakeTransform.localPosition = _originalPos + _perlinNoise * _currentShakeAmount;
        }

        /// <summary>
        /// Starts continious shake, note if you will run new shake then it will continue after new one finished <br>
        /// Use StopContiniousShake to stop it</br> 
        /// </summary>
        public void DoContiniousShake(ShakeSnapshoot snapshoot) {
            DoContiniousShake(snapshoot.ShakeAmount, snapshoot.Frequency, snapshoot.ShakeTimeIn, snapshoot.ShakeTimeOut, snapshoot.TimeDependent, snapshoot.Seed);
        }

        /// <summary>
        /// Starts continious shake, note if you will run new shake then it will continue after new one finished <br>
        /// Use StopContiniousShake to stop it</br> 
        /// </summary>
        public void DoContiniousShake(float shakeAmount, float frequency = 25f, float attackTime = 0.2f,
                                      float releaseTime = 0.5f, bool timeDependent = true, float seed = -1) {

            DoShake(shakeAmount, frequency, attackTime, float.MaxValue, releaseTime, timeDependent, true, seed);

            if (_ignoreShake == false) {
                _shakeSnapshoot = new ShakeSnapshoot() {
                    ShakeAmount = shakeAmount,
                    Frequency = frequency,
                    TimeDependent = timeDependent,
                    ShakeTimeIn = attackTime,
                    ShakeTimeDecay = float.MaxValue,
                    ShakeTimeOut = releaseTime,
                    Seed = _seed,
                };
                _continiousShakeOn = true;
            }
        }

        /// <summary>
        /// Stops continious shake, has no effects if there is no continious shake
        /// </summary>
        public void StopContiniousShake(bool immediately = false) {
            if (_continiousShakeOn) {
                _timerDecay = float.MaxValue;
                _continiousShakeOn = false;

                if (immediately) {
                    StopShake();
                }
            }
        }

        /// <summary>
        /// Short func version of shake effect
        /// </summary>
        public void DoFastShake(float shakeAmount, float frequency = 25f, bool overrideCurrent = true, bool timeDependent = true) {
            DoShake(shakeAmount, frequency, attackTime: 0.2f, decayTime: 0.5f, releaseTime: 0.3f, timeDependent, overrideCurrent);
        }

        public void DoShake(ShakeSnapshoot shakeSnapshoot, int seed = -1) {
            DoShake(shakeSnapshoot.ShakeAmount, shakeSnapshoot.Frequency, shakeSnapshoot.ShakeTimeIn, shakeSnapshoot.ShakeTimeDecay, shakeSnapshoot.ShakeTimeOut,
                    shakeSnapshoot.TimeDependent, shakeSnapshoot.OverrideShakeIfRunning, seed);
        }

        /// <summary>
        /// Turns on shake effect for a while
        /// </summary>
        /// <param name="shakeAmount">shake effect max distance from original pos</param>
        /// <param name="attackTime">shake effect go in time</param>
        /// <param name="decayTime">shake effect duration after go in time</param>
        /// <param name="releaseTime">shake effect go out time</param>
        /// <param name="timeDependent">if false - will be used unscaled delta time</param>
        /// <param name="overrideCurrent">if false - shake will be ignored if it's already enabled</param>
        public void DoShake(float shakeAmount, float frequency = 25f, float attackTime = 0.2f, float decayTime = 1f,
                                               float releaseTime = 0.5f, bool timeDependent = true, bool overrideCurrent = true, float seed = -1) {
            if (enabled && !overrideCurrent) {
                return;
            }

            if (_ignoreShake) {
#if UNITY_EDITOR
                Debug.LogWarning("ignore shake is true, so return", this);
#endif
                return;
            }

            ResetValues();

            if (enabled) ObjShakeTransform.localPosition = _originalPos;
            _originalPos = ObjShakeTransform.localPosition;
            _shakeAmount = shakeAmount;
            _shakeTimeIn = attackTime;
            _shakeTimeDecay = decayTime;
            _shakeTimeOut = releaseTime;
            _timeDependent = timeDependent;
            _frequency = frequency;
            if (seed < 0) {
                _seed = Random.value * 10f; // * 10f to increase range of _seed
            }

            enabled = true;
        }

        /// <summary>
        /// Immediately stops shake effect
        /// </summary>
        public void StopShake() {
            enabled = false;
            ObjShakeTransform.localPosition = _originalPos;
        }

        /// <summary>
        /// Stops active shake effect with it's release time
        /// </summary>
        public void StopShakeWithReleaseTime() {
            if (enabled) {
                _timerIn = _shakeTimeIn;
                _timerDecay = _shakeTimeDecay;
            }
        }

        /// <summary>
        /// Use it if you want to override current easings of in and out shake phases
        /// </summary>
        /// <param name="inShakeCurve">will be ignored if null</param>
        /// <param name="outShakeCurve">will be ignored if null</param>
        public void SetShakeInOutCurves(AnimationCurve inShakeCurve, AnimationCurve outShakeCurve) {
            if (inShakeCurve != null) _shakeInCurve = inShakeCurve;
            if (outShakeCurve != null) _shakeOutCurve = outShakeCurve;
        }

        private void ResetValues() {
            _timerDecay = 0f;
            _timerIn = 0f;
            _timerOut = 0f;
            _currentShakeAmount = 0f;
        }
    }
}