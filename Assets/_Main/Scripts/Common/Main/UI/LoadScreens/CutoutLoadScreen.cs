using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core {
    public class CutoutLoadScreen : LoadScreen {
        public override LoadScreenState CurrentState { get; protected set; }
        public override float TransitionInTime { get => _transitionInTime; set => _transitionInTime = value; }
        public override float TransitionOutTime { get => _transitionOutTime; set => _transitionOutTime = value; }

        [SerializeField] private RectTransform _cutoutMask;
        [SerializeField] private Image _background;
        [SerializeField, Range(0.1f, 5f)] private float _transitionInTime = 1.5f;
        [SerializeField, Range(0.1f, 5f)] private float _transitionOutTime = 1.5f;
        [SerializeField] private AnimationCurve _transitionIn;
        [SerializeField] private AnimationCurve _transitionOut;
        [SerializeField] private Quaternion _startRotationOffset = Quaternion.identity;
        [SerializeField] private Quaternion _endRotationOffset = Quaternion.identity;

        private Coroutine _transition;
        private float _progress = 0f;
        private Vector2 _cutoutMaskTransitionInStartSize = Vector2.zero;


        public void SetTransitionColor(Color color) {
            _background.color = color;
        }

        public override void OnUpdateLoadProgress(float loadingProgressValue) { }

        internal override void ShowScreen(UnityAction callback = null) {
            if (gameObject.activeSelf == false) gameObject.SetActive(true);
            if (_cutoutMaskTransitionInStartSize.x == 0f) _cutoutMaskTransitionInStartSize = _cutoutMask.sizeDelta;

            CurrentState = LoadScreenState.Showing;
            OnLoaderShown.AddListener(() => {
                CurrentState = LoadScreenState.Active;
            });
            if (callback != null) OnLoaderShown.AddListener(callback);

            _transition = StartCoroutine(DoTransitionIn());
        }

        internal override void ShowScreenImmediately() {
            CurrentState = LoadScreenState.Active;

            if (gameObject.activeSelf == false) gameObject.SetActive(true);
            if (_cutoutMaskTransitionInStartSize.x == 0f) _cutoutMaskTransitionInStartSize = _cutoutMask.sizeDelta;

            CoroutineHelper.Stop(this, ref _transition);
            _cutoutMask.sizeDelta = Vector2.zero;
            _progress = 1f;

            if (CurrentState == LoadScreenState.Hiding) {
                OnLoaderHiden.RemoveAllListeners();
            }
            else if (CurrentState == LoadScreenState.Showing) {
                OnLoaderShown?.Invoke();
                OnLoaderShown.RemoveAllListeners();
            }
        }

        internal override void HideScreen(UnityAction callback = null) {
            if (_cutoutMaskTransitionInStartSize.x == 0f) _cutoutMaskTransitionInStartSize = _cutoutMask.sizeDelta;

            CurrentState = LoadScreenState.Hiding;
            OnLoaderHiden.AddListener(() => {
                gameObject.SetActive(false);
                CurrentState = LoadScreenState.Inactive;
            });
            if (callback != null) OnLoaderHiden.AddListener(callback);

            _transition = StartCoroutine(DoTransitionOut());
        }

        internal override void HideScreenImmediately() {
            if (_cutoutMaskTransitionInStartSize.x == 0f) _cutoutMaskTransitionInStartSize = _cutoutMask.sizeDelta;

            CurrentState = LoadScreenState.Inactive;
            CoroutineHelper.Stop(this, ref _transition);
            _cutoutMask.sizeDelta = _cutoutMaskTransitionInStartSize;
            _progress = 0f;

            if (CurrentState == LoadScreenState.Hiding) {
                OnLoaderHiden?.Invoke();
                OnLoaderHiden.RemoveAllListeners();
            }
            else if (CurrentState == LoadScreenState.Showing) {
                OnLoaderShown.RemoveAllListeners();
            }

            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        internal override void ForceStop() {
            CoroutineHelper.Stop(this, ref _transition);
            if (CurrentState == LoadScreenState.Hiding) {
                OnLoaderHiden.RemoveAllListeners();
            }
            else if (CurrentState == LoadScreenState.Showing) {
                OnLoaderShown.RemoveAllListeners();
            }
        }

        private IEnumerator DoTransitionIn() {
            float transitionTime = (1f - _progress) * _transitionInTime;
            if (transitionTime > 0.001f) {
                for (float i = _progress; i < 1f; i += Time.unscaledDeltaTime / transitionTime) {
                    _progress = i;
                    float evaluatedProgress = _transitionIn.Evaluate(i);
                    _cutoutMask.localRotation = Quaternion.Slerp(_startRotationOffset, _endRotationOffset, evaluatedProgress);
                    _cutoutMask.sizeDelta = Vector2.LerpUnclamped(_cutoutMaskTransitionInStartSize, Vector2.zero, evaluatedProgress);
                    yield return null;
                }
            }
            _progress = 1f;
            _cutoutMask.sizeDelta = Vector2.zero;
			yield return null; // this will wait while alpha updates to 1f value
            OnLoaderShown?.Invoke();
            OnLoaderShown.RemoveAllListeners();
        }

        private IEnumerator DoTransitionOut() {
            yield return null; // this fix issue that unscaledDeltaTime is too high on first frame that there is no animation
                               // when scene just been loaded

            float transitionTime = _progress * _transitionOutTime;
            if (transitionTime > 0.001f) {
                for (float i = _progress; i > 0f; i -= Time.unscaledDeltaTime / transitionTime) {
                    _progress = i;
                    float evaluatedProgress = _transitionIn.Evaluate(i);
                    _cutoutMask.localRotation = Quaternion.Slerp(_startRotationOffset, _endRotationOffset, evaluatedProgress);
                    _cutoutMask.sizeDelta = Vector2.LerpUnclamped(_cutoutMaskTransitionInStartSize, Vector2.zero, evaluatedProgress);
                    yield return null;
                }
            }
            _progress = 0f;
            _cutoutMask.sizeDelta = _cutoutMaskTransitionInStartSize;
            yield return null; // this will wait while alpha disappear
            OnLoaderHiden?.Invoke();
            OnLoaderHiden.RemoveAllListeners();
        }
    }
}