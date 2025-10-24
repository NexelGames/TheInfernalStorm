using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Core {
    public class ImageFillOutIn : LoadScreen {
        public override LoadScreenState CurrentState { get; protected set; }
        public override float TransitionInTime { get => _transitionInTime; set => _transitionInTime = value; }
        public override float TransitionOutTime { get => _transitionOutTime; set => _transitionOutTime = value; }
        public Image FillImage => _fillImage;

        [SerializeField] private Image _fillImage;
        [SerializeField, HideInInspector] private int _fillInOrigin = (int)Image.OriginHorizontal.Left;
        [SerializeField, HideInInspector] private int _fillOutOrigin = (int)Image.OriginHorizontal.Right;
        [SerializeField, Range(0.1f, 5f)] private float _transitionInTime = 1.5f;
        [SerializeField, Range(0.1f, 5f)] private float _transitionOutTime = 1.5f;
        [SerializeField] private AnimationCurve _transitionIn;
        [SerializeField] private AnimationCurve _transitionOut;

        private Coroutine _transition;


        public void SetTransitionColor(Color color) {
            _fillImage.color = color;
        }

        public void SetFillInOrigin(int fillOriginType) {
            _fillInOrigin = fillOriginType;
            if (CurrentState == LoadScreenState.Showing) _fillImage.fillOrigin = _fillInOrigin;
        }

        public void SetFillOutOrigin(int fillOriginType) {
            _fillOutOrigin = fillOriginType;
            if (CurrentState == LoadScreenState.Hiding) _fillImage.fillOrigin = _fillOutOrigin;
        }

        public void SetFillMethod(Image.FillMethod method) => _fillImage.fillMethod = method;

        public override void OnUpdateLoadProgress(float loadingProgressValue) { }

        internal override void ShowScreen(UnityAction callback = null) {
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
            _fillImage.fillAmount = 1f;
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
            _fillImage.fillAmount = 0f;

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
            _fillImage.fillOrigin = (int)_fillInOrigin;

            float transitionTime = (1f - _fillImage.fillAmount) * _transitionInTime;
            if (transitionTime > 0.001f) {
                for (float i = _fillImage.fillAmount; i < 1f; i += Time.unscaledDeltaTime / transitionTime) {
                    _fillImage.fillAmount = _transitionIn.Evaluate(i);
                    yield return null;
                }
            }

            _fillImage.fillAmount = 1f;
			yield return null; // this will wait while fill updates to 1f value
            OnLoaderShown?.Invoke();
            OnLoaderShown.RemoveAllListeners();
        }

        private IEnumerator DoTransitionOut() {
            yield return null; // this fix issue that unscaledDeltaTime is too high on first frame that there is no animation
                               // when scene just been loaded

            _fillImage.fillOrigin = (int)_fillOutOrigin;

            float transitionTime = _fillImage.fillAmount * _transitionOutTime;
            if (transitionTime > 0.001f) {
                for (float i = _fillImage.fillAmount; i > 0f; i -= Time.unscaledDeltaTime / transitionTime) {
                    _fillImage.fillAmount = _transitionOut.Evaluate(i);
                    yield return null;
                }
            }

            _fillImage.fillAmount = 0f;
			yield return null; // this will wait while fill disappear
            OnLoaderHiden?.Invoke();
            OnLoaderHiden.RemoveAllListeners();
        }
    }
}

