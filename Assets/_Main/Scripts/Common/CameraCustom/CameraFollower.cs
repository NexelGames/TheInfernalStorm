using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {
    /// <summary>
    /// CameraPOVData is used to keep position and rotation of each POV
    /// </summary>
    [Serializable]
    public class CameraPOVData {
        public string Name;
        public Vector3 CameraLocalPosition;
        public Quaternion CameraLocalRotation;
        public float FovOrOrhograpicSize = 60f;
    }

    [Serializable]
    public class CameraFocusPoint {
        public Transform Point;
        public bool CopyPointRotation = false;
        public float MoveDuration = 1f;
        public float OnReachedPointFocusDuration = 0.5f;
        public string POV_Name;
    }

    public class CameraFollower : MonoBehaviour {
        public static UnityEvent OnPOVChangeStart = new();
        public static UnityEvent OnPOVChangeEnd = new();

        public static UnityEvent OnFocusStart = new();
        public static UnityEvent<CameraFocusPoint> OnFocused = new();
        public static UnityEvent OnFocusFinished = new();

        /// <summary>
        /// Target that camera holder follows to
        /// </summary>
        public Transform FollowTarget {
            get => _followTarget;
            set {
                if (_coroutineChangeFollowTarget != null) {
                    StopCoroutine(_coroutineChangeFollowTarget);
                    _coroutineChangeFollowTarget = null;
                    _onChangeFollowTarget_Finished = null;
                }
                if (_focusingCoroutine != null) {
                    _lastFollowTarget = value;
                    _focusBeforeCoroutineStarted.Point = value;
                    return;
                }
                _followTarget = value;
            }
        }

        public CameraFollowLogic FollowLogic {
            get => _cameraFollowLogic;
            set {
                _cameraFollowLogic = value;
                if (_cameraFollowLogic != null && _cameraFollowLogic.Inited == false) {
                    _cameraFollowLogic.Init(this);
                }
            }
        }
        public bool FollowLogicControlHolderRotation {
            get {
                if (_cameraFollowLogic != null) return _cameraFollowLogic.ControlsHolderRotation;
                return false;
            }
            set {
                if (_cameraFollowLogic != null) _cameraFollowLogic.ControlsHolderRotation = value;
            }
        }

        public bool IsFocusing => _focusingCoroutine != null;

        /// <summary>
        /// Shake system, that is used to make Camera shake effects
        /// </summary>
        public ObjectShake ShakeEffect => _cameraShake;

        /// <summary>
        /// FOV handler, that is used to make FOV effects (change field of view runtime)
        /// </summary>
        public CameraFOVHandler CameraFOVHandler => _cameraFOV;

        /// <summary>
        /// Camera transform in holder system
        /// </summary>
        public Transform CameraTransform => _cameraTransform;

        public Camera CameraComponent => _cameraComponent;

        /// <summary>
        /// System camera and other modules holder transform (move it to keep main camera offsets=POVs)
        /// </summary>
        public Transform HolderTransform => _systemHolder;

        /// <summary>
        /// Bunch of holder local points, that we can refer to change it's local position for camera and rotation
        /// </summary>
        public CameraPOVData[] CameraPOV_s;
        /// <summary>
        /// Active local point in holder that is used as position and rotation for MainCamera object
        /// </summary>
        public CameraPOVData ActivePov => _activePOV;

        [SerializeField] protected Transform _followTarget;
        [SerializeField, SerializeReference] protected CameraFollowLogic _cameraFollowLogic;

        [Space(10f)]
        [SerializeField] protected Transform _cameraTransform;
        [SerializeField] protected Camera _cameraComponent;
        [SerializeField] protected ObjectShake _cameraShake;
        [SerializeField] protected CameraFOVHandler _cameraFOV;

        /// <summary>
        /// Used to make interpolated curve while changing POV from one to another<br>
        /// You can change it immediately after <see cref="ChangePov(POV_Names, float)"/> call</br>
        /// </summary>
        [Space(10f)]
        [SerializeField] protected AnimationCurve _changePOV_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        /// <summary>
        /// Used to make interpolated curve while changing POV from one to another<br>
        /// You can change it immediately after <see cref="ChangePov(POV_Names, float)"/> call</br>
        /// </summary>
        [SerializeField] protected AnimationCurve _changePOV_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] protected AnimationCurve _focusPointMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Used to make interpolated curve while changing FollowTarget from one to another<br>
        /// You can change it immediately after <see cref="ChangeFollowTargetCoroutine(Transform, float, float)"/> call</br>
        /// </summary>
        [Space(10f)]
        [SerializeField] protected AnimationCurve _changeTarget_PositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        /// <summary>
        /// Used to make interpolated curve while changing FollowTarget from one to another<br>
        /// You can change it immediately after <see cref="ChangeFollowTargetCoroutine(Transform, float, float)"/> call</br>
        /// </summary>
        [SerializeField] protected AnimationCurve _changeTarget_RotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        protected Coroutine _coroutineChangePOV;
        protected Coroutine _coroutineChangeFollowTarget;
        protected Coroutine _focusingCoroutine;

        protected Transform _systemHolder;
        protected Transform _cameraHolder;
        protected CameraPOVData _activePOV;

        protected AnimationCurve _defaultChangePOV_PosCurve;
        protected AnimationCurve _defaultChangePOV_RotCurve;
        protected AnimationCurve _defaultChangeTargetPosCurve;
        protected AnimationCurve _defaultChangeTargetRotCurve;

        protected CameraLens _cameraLens;
        protected Transform _lastFollowTarget;
        protected Transform _emptyPointFrom;

        protected List<CameraFocusPoint> _focusPoints = new(2);
        protected CameraFocusPoint _focusBeforeCoroutineStarted;

        protected Action _onPOV_Changed;
        protected Action _onChangeFollowTarget_Finished;

        protected virtual void Awake() {
            _systemHolder = transform;
            _cameraHolder = _cameraTransform.parent;

            if (transform.parent != null) {
                transform.parent = null;
            }

            _emptyPointFrom = new GameObject("CameraFollower EmptyPointFrom").transform;
            _emptyPointFrom.parent = _systemHolder.parent;

            _defaultChangePOV_PosCurve = _changePOV_PositionCurve;
            _defaultChangePOV_RotCurve = _changePOV_RotationCurve;

            _defaultChangeTargetPosCurve = _changeTarget_PositionCurve;
            _defaultChangeTargetRotCurve = _changeTarget_RotationCurve;

            FollowLogic = _cameraFollowLogic;

            if (_cameraComponent == null) {
                _cameraComponent = Camera.main;
            }

            if (_cameraComponent.transform.parent != null) {
                _cameraComponent.transform.SetParent(null, false);
            }

            _cameraFOV.Init();
            _cameraLens = _cameraFOV.CameraLens;
            ChangePov(CameraPOV_s[0], 0f);
        }

        protected void LateUpdate() {
            if (_followTarget) {
                if (_cameraFollowLogic != null) {
                    _cameraFollowLogic.OnLateUpdate(_followTarget);
                }
            }
        }

        public CameraFollower SetSystemLocalPositionAndRotationButKeepCamera(Vector3 localPosition, Quaternion localRotation) {
            Quaternion cameraRotationBefore = _cameraHolder.rotation;
            Vector3 cameraPositionBeforeRotation = _cameraHolder.position;

            _systemHolder.localRotation = localRotation;
            _systemHolder.localPosition = localPosition;
            _cameraHolder.rotation = cameraRotationBefore;
            _cameraHolder.localPosition = _systemHolder.InverseTransformPoint(cameraPositionBeforeRotation);
            return this;
        }

        public CameraFollower SetSystemLocalRotationButKeepCamera(Quaternion localRotation) {
            Quaternion cameraRotationBefore = _cameraHolder.rotation;
            Vector3 cameraPositionBeforeRotation = _cameraHolder.position;

            _systemHolder.localRotation = localRotation;
            _cameraHolder.rotation = cameraRotationBefore;
            _cameraHolder.localPosition = _systemHolder.InverseTransformPoint(cameraPositionBeforeRotation);
            return this;
        }

        public CameraFollower SetSystemLocalPositionButKeepCamera(Vector3 localPosition) {
            Vector3 cameraDelta = _systemHolder.localPosition - localPosition;

            _systemHolder.localPosition = localPosition;
            _cameraHolder.position += cameraDelta;
            return this;
        }

        public CameraPOVData GetPOV(string povName) {
            var index = GetPOV_Index(povName);
            if (index == -1) {
                return null;
            }
            return CameraPOV_s[index];
        }

        public void AddFocusPoint(Transform point, float focusTime = 1f) {
            AddFocusPoint(new CameraFocusPoint() { Point = point, OnReachedPointFocusDuration = focusTime });
        }

        public void AddFocusPoint(CameraFocusPoint focusPoint) {
            _focusPoints.Add(focusPoint);

            if (_focusingCoroutine == null) {
                _lastFollowTarget = _followTarget;
                _followTarget = null;

                if (_focusBeforeCoroutineStarted == null) {
                    _focusBeforeCoroutineStarted = new CameraFocusPoint();
                }
                _focusBeforeCoroutineStarted.POV_Name = _activePOV.Name;
                _focusingCoroutine = StartCoroutine(FocusPoints());
            }
        }

        IEnumerator FocusPoints() {
            OnFocusStart.Invoke();

            CameraFocusPoint focusPoint;
            float easedValue;
            Quaternion initRotation = _systemHolder.rotation;

            while (_focusPoints.Count > 0) {
                focusPoint = _focusPoints[0];
                _emptyPointFrom.position = _systemHolder.position;
                _emptyPointFrom.rotation = _systemHolder.rotation;

                if (string.IsNullOrEmpty(focusPoint.POV_Name) == false) {
                    ChangePov(focusPoint.POV_Name, focusPoint.MoveDuration - Time.deltaTime);
                }

                for (float i = 0f; i < 1f; i += Time.deltaTime / focusPoint.MoveDuration) {
                    easedValue = _focusPointMoveCurve.Evaluate(i);
                    _systemHolder.position = Vector3.Lerp(
                                            _systemHolder.position,
                                            Vector3.Lerp(_emptyPointFrom.position, focusPoint.Point.position, easedValue),
                                            easedValue
                                        );

                    if (focusPoint.CopyPointRotation) {
                        _systemHolder.rotation = Quaternion.Slerp(
                            _systemHolder.rotation,
                            Quaternion.Slerp(_emptyPointFrom.rotation, focusPoint.Point.rotation, easedValue),
                                easedValue
                            );
                    }
                    yield return null;
                }

                _systemHolder.position = focusPoint.Point.position;
                if (focusPoint.CopyPointRotation) _systemHolder.rotation = focusPoint.Point.rotation;

                OnFocused.Invoke(focusPoint);
                _focusPoints.RemoveAt(0);
                yield return new WaitForSeconds(focusPoint.OnReachedPointFocusDuration);
            }

            if (_lastFollowTarget == null) {
                _focusingCoroutine = null;
                OnFocusFinished.Invoke();
                yield break;
            }

            // go back
            focusPoint = _focusBeforeCoroutineStarted;
            focusPoint.Point = _lastFollowTarget;
            _emptyPointFrom.position = _systemHolder.position;
            _emptyPointFrom.rotation = _systemHolder.rotation;

            if (_activePOV.Name.Equals(focusPoint.POV_Name) == false) {
                ChangePov(focusPoint.POV_Name, focusPoint.MoveDuration - Time.deltaTime);
            }

            if (_cameraFollowLogic != null) {
                for (float i = 0f; i < 1f; i += Time.deltaTime / focusPoint.MoveDuration) {
                    easedValue = _focusPointMoveCurve.Evaluate(i);

                    if (_cameraFollowLogic.ControlsHolderRotation) {
                        _cameraFollowLogic.OnFocusTransitionToFollow(_emptyPointFrom, _lastFollowTarget.position, _lastFollowTarget.rotation, easedValue);
                    }
                    else {
                        _cameraFollowLogic.OnFocusTransitionToFollow(_emptyPointFrom, _lastFollowTarget.position, initRotation, easedValue);
                    }
                    yield return null;
                }

                if (_cameraFollowLogic.ControlsHolderRotation) {
                    _cameraFollowLogic.OnFocusTransitionToFollow(_emptyPointFrom, _lastFollowTarget.position, _lastFollowTarget.rotation, 1f);
                }
                else {
                    _cameraFollowLogic.OnFocusTransitionToFollow(_emptyPointFrom, _lastFollowTarget.position, initRotation, 1f);
                }
            }
            else {
                for (float i = 0f; i < 1f; i += Time.deltaTime / focusPoint.MoveDuration) {
                    easedValue = _focusPointMoveCurve.Evaluate(i);

                    _systemHolder.position = Vector3.Lerp(
                        _systemHolder.position,
                        Vector3.Lerp(_emptyPointFrom.position, _lastFollowTarget.position, easedValue),
                        easedValue
                    );

                    if (initRotation != _systemHolder.rotation) {
                        _systemHolder.rotation = Quaternion.Slerp(
                            _systemHolder.rotation,
                            Quaternion.Slerp(_emptyPointFrom.rotation, initRotation, easedValue),
                            easedValue
                        );
                    }
                    yield return null;
                }

                _systemHolder.position = _lastFollowTarget.position;
                _systemHolder.rotation = initRotation;
            }

            if (_focusPoints.Count > 0) {
                StartCoroutine(FocusPoints());
                yield break;
            }

            _focusingCoroutine = null;
            FollowTarget = _lastFollowTarget;
            OnFocusFinished.Invoke();
        }

        /// <summary>
        /// Smoothly changes follow target by <paramref name="changeTime"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangeTarget(Transform newTarget, float changeTime = 0f, float changeDelay = 0f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (changeTime <= 0f) {
                FollowTarget = newTarget;
                _systemHolder.position = newTarget.position;
                return this;
            }

            if (_coroutineChangeFollowTarget != null) {
                StopCoroutine(_coroutineChangeFollowTarget);
                _onChangeFollowTarget_Finished = null;
            }

            if (positionCurve == null) {
                _changeTarget_PositionCurve = _defaultChangeTargetPosCurve;
            }
            else {
                _changeTarget_PositionCurve = positionCurve;
            }

            if (rotationCurve == null) {
                _changeTarget_RotationCurve = _defaultChangeTargetRotCurve;
            }
            else {
                _changeTarget_RotationCurve = rotationCurve;
            }

            _coroutineChangeFollowTarget = StartCoroutine(ChangeFollowTargetCoroutine(newTarget, changeTime, changeDelay));
            return this;
        }

        /// <summary>
        /// Appends callback to running Change follow target process. <br>
        /// If function ChangeTarget wasnt called, then callback will be invoked immediately </br>
        /// </summary>
        public void AppendOnChangeTarget_FinishedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangeFollowTarget == null) {
                callback.Invoke();
            }
            _onChangeFollowTarget_Finished += callback;
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(int povIndex, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (CameraPOV_s.Length - 1 < povIndex) {
                Debug.LogError($"POV number {povIndex + 1} doesn't exist");
                return this;
            }
            if (CameraPOV_s[povIndex].Name.Equals(_activePOV.Name)) {
                return this;
            }
            return ChangePov(CameraPOV_s[povIndex], time, positionCurve, rotationCurve);
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(string povName, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (povName.Equals(_activePOV.Name)) {
                return this;
            }
            int povIndex = GetPOV_Index(povName);
            return ChangePov(CameraPOV_s[povIndex], time, positionCurve, rotationCurve);
        }

        /// <summary>
        /// Smoothly changes POV by <paramref name="time"/>
        /// </summary>
        /// <returns>Camera Follower, so you can add subscribe to callbacks and etc</returns>
        public CameraFollower ChangePov(CameraPOVData cameraPOV, float time = 1f,
                AnimationCurve positionCurve = null, AnimationCurve rotationCurve = null) {
            if (_activePOV != null && _activePOV.Name.Equals(cameraPOV.Name)) {
                return this;
            }

            _activePOV = cameraPOV;
            OnPOVChangeStart.Invoke();

            if (_coroutineChangePOV != null) {
                _cameraFOV.StopChange();
                StopCoroutine(_coroutineChangePOV);
                _onPOV_Changed = null;
            }

            if (time <= 0f) {
                _cameraHolder.localPosition = _activePOV.CameraLocalPosition;
                _cameraHolder.localRotation = _activePOV.CameraLocalRotation;

                _cameraFOV.StopChange();
                if (_cameraLens.orthographic == false) {
                    _cameraFOV.ChangeFOV(cameraPOV.FovOrOrhograpicSize);
                }
                else {
                    _cameraFOV.ChangeOrthographicSize(cameraPOV.FovOrOrhograpicSize);
                }

                OnPOVChangeEnd.Invoke();
                return this;
            }

            if (positionCurve == null) {
                _changePOV_PositionCurve = _defaultChangePOV_PosCurve;
            }
            else {
                _changePOV_PositionCurve = positionCurve;
            }

            if (rotationCurve == null) {
                _changePOV_RotationCurve = _defaultChangePOV_RotCurve;
            }
            else {
                _changePOV_RotationCurve = rotationCurve;
            }

            _cameraShake.IgnoreShake = true;

            if (_cameraLens.orthographic == false) {
                _cameraFOV.ChangeFOV(cameraPOV.FovOrOrhograpicSize, time);
            }
            else {
                _cameraFOV.ChangeOrthographicSize(cameraPOV.FovOrOrhograpicSize, time);
            }

            _coroutineChangePOV = StartCoroutine(POVChanging(_activePOV, time));
            return this;
        }

        public static Sequence ChangePOVCinemachine(Transform cameraRoot, CinemachineCamera camera, CameraPOVData toPOV, float duration) {
            if (duration <= 0f) {
                camera.transform.SetPositionAndRotation(cameraRoot.TransformPoint(toPOV.CameraLocalPosition), cameraRoot.rotation * toPOV.CameraLocalRotation);
                return default;
            }

            var seq = Sequence.Create();
            seq.Chain(Tween.Position(camera.transform, cameraRoot.TransformPoint(toPOV.CameraLocalPosition), duration));
            seq.Group(Tween.Rotation(camera.transform, cameraRoot.rotation * toPOV.CameraLocalRotation, duration));
            seq.Group(Tween.Custom(camera, camera.Lens.FieldOfView, toPOV.FovOrOrhograpicSize, duration,
                                   (cinemachineCamera, fov) => cinemachineCamera.Lens.FieldOfView = fov));

            return seq;
        }

        /// <summary>
        /// Set callback to running POV change process <br>
        /// This method will reset all callbacks and set argument <br></br>
        /// If no ChangePov was called, then callback will be invoked immediately </br>
        /// </summary>
        public void SetOnPOV_ChangedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangePOV == null) {
                callback.Invoke();
                return;
            }
            _onPOV_Changed = callback;
        }

        /// <summary>
        /// Appends callback to running POV change process. <br>
        /// If no ChangePov was called, then callback will be invoked immediately </br>
        /// </summary>
        public void AppendOnPOV_ChangedCallback(Action callback) {
            if (callback == null) {
                return;
            }

            if (_coroutineChangePOV == null) {
                callback.Invoke();
                return;
            }
            _onPOV_Changed += callback;
        }

        IEnumerator POVChanging(CameraPOVData cameraPOV, float time) {
            Vector3 startPos = _cameraHolder.localPosition;
            Quaternion startRot = _cameraHolder.localRotation;

            // if you want to change POV even if time freezed -> use Time.unscaledDeltaTime
            for (float i = 0f; i < 1f; i += Time.deltaTime / time) {
                _cameraHolder.localPosition =
                        Vector3.Lerp(startPos, cameraPOV.CameraLocalPosition, _changePOV_PositionCurve.Evaluate(i));
                _cameraHolder.localRotation =
                        Quaternion.Slerp(startRot, cameraPOV.CameraLocalRotation, _changePOV_RotationCurve.Evaluate(i));
                yield return null;
            }
            _cameraHolder.localPosition = cameraPOV.CameraLocalPosition;
            _cameraHolder.localRotation = cameraPOV.CameraLocalRotation;
            _coroutineChangePOV = null;
            _cameraShake.IgnoreShake = false;


            var callback = _onPOV_Changed;
            _onPOV_Changed = null;
            callback?.Invoke();

            OnPOVChangeEnd.Invoke();
        }

        IEnumerator ChangeFollowTargetCoroutine(Transform newTarget,
                                                float changeDuration, float changeDelay = 0f) {
            if (changeDelay > 0f) {
                yield return new WaitForSeconds(changeDelay);
            }

            _emptyPointFrom.SetPositionAndRotation(_systemHolder.position, _systemHolder.rotation);

            //to prevent LateUpdates;
            _followTarget = null;

            if (_cameraFollowLogic != null) {
                for (float i = 0; i < 1f; i += Time.deltaTime / changeDuration) {
                    _cameraFollowLogic.OnFollowTargetTransition(_emptyPointFrom, newTarget,
                                                                _changeTarget_PositionCurve.Evaluate(i),
                                                                _changeTarget_RotationCurve.Evaluate(i));
                    yield return null;
                }
                _cameraFollowLogic.OnFollowTargetTransition(_emptyPointFrom, newTarget, 1f, 1f);
            }

            _coroutineChangeFollowTarget = null;

            FollowTarget = newTarget;

            var callback = _onChangeFollowTarget_Finished;
            _onChangeFollowTarget_Finished = null;
            callback?.Invoke();
        }

        protected int GetPOV_Index(string name) {
            int index = 0;
            foreach (var pov in CameraPOV_s) {
                if (string.Equals(pov.Name, name, StringComparison.Ordinal)) {
                    return index;
                }
                index++;
            }
#if UNITY_EDITOR
            Debug.LogWarning($"Didnt find POV with name {name}.");
#endif
            return -1;
        }

        protected void OnValidate() {
            if (CameraPOV_s == null || CameraPOV_s.Length == 0 && _cameraHolder != null) {
                CameraPOV_s = new CameraPOVData[1] {
                    new CameraPOVData() {
                        Name = "Default",
                        CameraLocalPosition = _cameraHolder.localPosition,
                        CameraLocalRotation = _cameraHolder.rotation
                    }
                };
            }
        }
    }
}