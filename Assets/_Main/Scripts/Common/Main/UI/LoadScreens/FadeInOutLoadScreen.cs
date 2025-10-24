using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core {
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeInOutLoadScreen : LoadScreen {
        public override LoadScreenState CurrentState { get; protected set; }
        public override float TransitionInTime { get => _transitionInTime; set => _transitionInTime = value; }
        public override float TransitionOutTime { get => _transitionOutTime; set => _transitionOutTime = value; }

        [SerializeField] private Image _backgroundImage;
        [SerializeField, Range(0.1f, 5f)] private float _transitionInTime = 1.5f;
        [SerializeField, Range(0.1f, 5f)] private float _transitionOutTime = 1.5f;
        [SerializeField] private AnimationCurve _transitionIn;
        [SerializeField] private AnimationCurve _transitionOut;

        private Coroutine _transition;
        private CanvasGroup _canvasGroup;


        public void SetTransitionColor(Color color) {
            _backgroundImage.color = color;
        }

        public override void OnUpdateLoadProgress(float loadingProgressValue) { }

        internal override void ShowScreen(UnityAction callback = null) {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            if (gameObject.activeSelf == false) gameObject.SetActive(true);

            CurrentState = LoadScreenState.Showing;
            OnLoaderShown.AddListener(() => {
                CurrentState = LoadScreenState.Active;
            });
            if (callback != null) OnLoaderShown.AddListener(callback);

            _transition = StartCoroutine(DoTransitionIn());
        }

        internal override void ShowScreenImmediately() {
            CurrentState = LoadScreenState.Active;
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;
            if (gameObject.activeSelf == false) gameObject.SetActive(true);

            CoroutineHelper.Stop(this, ref _transition);
            if (CurrentState == LoadScreenState.Hiding) {
                OnLoaderHiden.RemoveAllListeners();
            }
            else if (CurrentState == LoadScreenState.Showing) {
                OnLoaderShown?.Invoke();
                OnLoaderShown.RemoveAllListeners();
            }
        }

        internal override void HideScreen(UnityAction callback = null) {
            CurrentState = LoadScreenState.Hiding;
            OnLoaderHiden.AddListener(() => {
                gameObject.SetActive(false);
                CurrentState = LoadScreenState.Inactive;
            });
            if (callback != null) OnLoaderHiden.AddListener(callback);

            _transition = StartCoroutine(DoTransitionOut());
        }

        internal override void HideScreenImmediately() {
            CurrentState = LoadScreenState.Inactive;
            _canvasGroup.alpha = 0f;

            CoroutineHelper.Stop(this, ref _transition);
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
            float transitionTime = (1f - _canvasGroup.alpha) * _transitionInTime;
            if (transitionTime > 0.001f) {
                for (float i = _canvasGroup.alpha; i < 1f; i += Time.unscaledDeltaTime / transitionTime) {
                    _canvasGroup.alpha = _transitionIn.Evaluate(i);
                    yield return null;
                }
            }
            _canvasGroup.alpha = _transitionIn.Evaluate(1f);
			yield return null; // this will wait while alpha updates to 1f value
            OnLoaderShown?.Invoke();
            OnLoaderShown.RemoveAllListeners();
        }

        private IEnumerator DoTransitionOut() {
            yield return null; // this fix issue that unscaledDeltaTime is too high on first frame that there is no animation
                               // when scene just been loaded

            float transitionTime = _canvasGroup.alpha * _transitionOutTime;
            if (transitionTime > 0.001f) {
                for (float i = _canvasGroup.alpha; i > 0f; i -= Time.unscaledDeltaTime / transitionTime) {
                    _canvasGroup.alpha = _transitionOut.Evaluate(i);
                    yield return null;
                }
            }
            _canvasGroup.alpha = _transitionOut.Evaluate(0f);
			yield return null; // this will wait while alpha disappear
            OnLoaderHiden?.Invoke();
            OnLoaderHiden.RemoveAllListeners();
        }
    }
}