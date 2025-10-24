using UnityEngine;
using PrimeTween;
using Unity.Cinemachine;

namespace Game.Core {
    public class CameraFOVHandler : MonoBehaviour {
        [SerializeField] private AnimationCurve _changeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <summary>
        /// Current field of view of camera
        /// </summary>
        public float CurrentFOV => _targetLens.fieldOfView;

        /// <summary>
        /// Current OrthographicSize
        /// </summary>
        public float CurrentOrthographicSize => _targetLens.orthographicSize;

        public CameraLens CameraLens => _targetLens;

        private float _initFOV;
        private float _initOrthograpicSize;

        private CameraLens _targetLens;
        private Sequence _contolSequence;
        private bool _inited = false;


        public void Awake() {
            if (_inited == false) {
                Init();
            }
        }

        public void Init() {
            if (TryGetComponent(out Camera defaultCamera)) _targetLens = new DefaultCameraLens(defaultCamera);
            else if (TryGetComponent(out CinemachineCamera virtualCamera)) _targetLens = new CinemachineCameraLens(virtualCamera);

#if UNITY_EDITOR
            if (_targetLens == null) {
                Debug.LogError("No valid camera found for FOV handler.", gameObject);
                return;
            }
#endif

            _initFOV = _targetLens.fieldOfView;
            _initOrthograpicSize = _targetLens.orthographicSize;

            _inited = true;
        }

        public Vector2 GetOrthographicBounds() {
            float cameraHeight = _targetLens.orthographicSize * 2;
            return new Vector2(cameraHeight * _targetLens.aspect, cameraHeight);
        }

        public Vector2 GetFrustrumBounds(float atDistance) {
            float frustumHeight = 2.0f * atDistance * Mathf.Tan(_targetLens.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * _targetLens.aspect, frustumHeight);
        }

        /// <summary>
        /// Generic method for FOV or Orthographic bounce, depends on current camera orthographic property
        /// </summary>
        public void DoBounce(float bounceDelta, float durationIn, float durationOut, float endTarget = -1f) {
            if (_targetLens.orthographic == false) {
                DoFOVBounce(bounceDelta, durationIn, durationOut, endTarget);
            }
            else {
                DoOrthographicSizeBounce(bounceDelta, durationIn, durationOut, endTarget);
            }
        }

        /// <summary>
        /// Generic method for FOV or Orthographic bounce, depends on current camera orthographic property
        /// </summary>
        public void DoBounce(float bounceDelta, float durationIn, float durationOut, Ease easeIn, Ease easeOut, float endTarget = -1f) {
            if (_targetLens.orthographic == false) {
                DoFOVBounce(bounceDelta, durationIn, durationOut, easeIn, easeOut, endTarget);
            }
            else {
                DoOrthographicSizeBounce(bounceDelta, durationIn, durationOut, easeIn, easeOut, endTarget);
            }
        }

        /// <summary>
        /// Reset FOV to init default
        /// </summary>
        public void ResetFOV(float duration, AnimationCurve curve = null) {
            ChangeFOV(_initFOV, duration, curve);
        }

        /// <summary>
        /// Reset FOV to init default
        /// </summary>
        public void ResetFOV() {
            StopChange();
            ChangeFOV(_initFOV);
        }

        public void ChangeFOV(float target) {
            _targetLens.fieldOfView = target;
        }

        public void ChangeFOV(float to, float changeTime, AnimationCurve curve = null) {
            StopChange();
            if (curve == null) { curve = _changeCurve; }
            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.fieldOfView, to, changeTime, ChangeFOV, curve))
                           .OnComplete(target: this, target => target._targetLens.fieldOfView = to);
        }

        public void ChangeFOV(float to, float changeTime, Ease ease) {
            StopChange();
            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.fieldOfView, to, changeTime, ChangeFOV, ease))
                           .OnComplete(target: this, target => target._targetLens.fieldOfView = to);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue) {
            ChangeFOV(_targetLens.fieldOfView + fovAddValue);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue, float changeTime, AnimationCurve curve = null) {
            ChangeFOV(_targetLens.fieldOfView + fovAddValue, changeTime, curve);
        }

        /// <summary>
        /// Adds FOV to current camera FOV value
        /// </summary>
        public void AddFOV(float fovAddValue, float changeTime, Ease ease) {
            ChangeFOV(_targetLens.fieldOfView + fovAddValue, changeTime, ease);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoFOVBounce(float bounceDeltaFOV, float durationIn, float durationOut, float endTarget = -1f) {
            StopChange();

            float endValue = _targetLens.fieldOfView;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget >= 0) {
                endValue = endTarget;
            }

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.fieldOfView, bounceFOV, durationIn, ChangeFOV, _changeCurve))
                           .Chain(Tween.Custom(bounceFOV, endValue, durationOut, ChangeFOV, _changeCurve))
                           .OnComplete(target: this, target => target._targetLens.fieldOfView = endValue);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoFOVBounce(float bounceDeltaFOV, float durationIn, float durationOut, Ease easeIn, Ease easeOut, float endTarget = -1f) {
            StopChange();

            float endValue = _targetLens.fieldOfView;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget >= 0) {
                endValue = endTarget;
            }

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.fieldOfView, bounceFOV, durationIn, ChangeFOV, easeIn))
                           .Chain(Tween.Custom(bounceFOV, endValue, durationOut, ChangeFOV, easeOut))
                           .OnComplete(target: this, target => target._targetLens.fieldOfView = endValue);
        }


        /// <summary>
        /// Reset OrthographicSize to init default
        /// </summary>
        public void ResetOrthographicSize(float duration, AnimationCurve curve = null) {
            ChangeOrthographicSize(_initOrthograpicSize, duration, curve);
        }

        public void ResetOrthographicSize() {
            StopChange();
            ChangeOrthographicSize(_initOrthograpicSize);
        }

        public void ChangeOrthographicSize(float target) {
            _targetLens.orthographicSize = target;
        }

        public void ChangeOrthographicSizeWorldX(float xViewWorldSize) {
            _targetLens.orthographicSize = xViewWorldSize / _targetLens.aspect * 0.5f;
        }

        public void ChangeOrthographicSizeWorldX(float xViewWorldSize, float changeTime, AnimationCurve curve = null) {
            ChangeOrthographicSize(xViewWorldSize / _targetLens.aspect * 0.5f, changeTime, curve);
        }

        public void ChangeOrthographicSize(float to, float changeTime, AnimationCurve curve = null) {
            StopChange();
            if (curve == null) { curve = _changeCurve; }

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.orthographicSize, to, changeTime, ChangeOrthographicSize, curve))
                           .OnComplete(target: this, target => target._targetLens.orthographicSize = to);

        }

        public void ChangeOrthographicSize(float to, float changeTime, Ease ease) {
            StopChange();

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.orthographicSize, to, changeTime, ChangeOrthographicSize, ease))
                           .OnComplete(target: this, target => target._targetLens.orthographicSize = to);

        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue) {
            ChangeOrthographicSize(_targetLens.orthographicSize + addValue);
        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue, float changeTime, AnimationCurve curve = null) {
            ChangeOrthographicSize(_targetLens.orthographicSize + addValue, changeTime, curve);
        }

        /// <summary>
        /// Adds OrthographicSize to current camera OrthographicSize value
        /// </summary>
        public void AddOrthographicSize(float addValue, float changeTime, Ease ease) {
            ChangeOrthographicSize(_targetLens.orthographicSize + addValue, changeTime, ease);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoOrthographicSizeBounce(float bounceDeltaFOV, float durationIn, float durationOut, float endTarget = -1f) {
            StopChange();

            float endValue = _targetLens.orthographicSize;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget > 0) {
                endValue = endTarget;
            }

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.orthographicSize, bounceFOV, durationIn, ChangeOrthographicSize, _changeCurve))
                           .Chain(Tween.Custom(bounceFOV, endValue, durationOut, ChangeOrthographicSize, _changeCurve))
                           .OnComplete(target: this, target => target._targetLens.orthographicSize = endValue);
        }

        /// <param name="bounceDeltaFOV">value that added to current FOV as amplitude. For example 5 will add to caemra FOV (60+5) and then back to 60, where 60 is current FOV</param>
        /// <param name="endTarget">left -1 to keep scale before bounce, or set value to back to new target after bounce</param>
        public void DoOrthographicSizeBounce(float bounceDeltaFOV, float durationIn, float durationOut, Ease easeIn, Ease easeOut, float endTarget = -1f) {
            StopChange();

            float endValue = _targetLens.orthographicSize;
            float bounceFOV = endValue + bounceDeltaFOV;
            if (endTarget > 0) {
                endValue = endTarget;
            }

            _contolSequence = Sequence.Create();
            _contolSequence.Chain(Tween.Custom(_targetLens.orthographicSize, bounceFOV, durationIn, ChangeOrthographicSize, easeIn))
                           .Chain(Tween.Custom(bounceFOV, endValue, durationOut, ChangeOrthographicSize, easeOut))
                           .OnComplete(target: this, target => target._targetLens.orthographicSize = endValue);
        }

        public void StopChange(bool complete = true) {
            if (complete) _contolSequence.Complete();
            else _contolSequence.Stop();
        }

        private void OnDestroy() {
            StopChange();
        }
    }
}