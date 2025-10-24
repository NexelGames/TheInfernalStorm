using Alchemy.Inspector;
using Game.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    [AddComponentMenu("Common/UI/Canvas Move Transition")]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasMovableTransition : MonoBehaviour, ICanvasTransition {
        public bool IsRunning => _enableFadeCoroutine != null;

        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private UIMoveAnimationData[] _movableElements;
        [SerializeField] private bool _alphaFadeOnTranstion = true;
        [SerializeField, ShowIf("_alphaFadeOnTranstion")]
        private AnimationCurve _alphaAppearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, ShowIf("_alphaFadeOnTranstion")]
        private AnimationCurve _alphaHideCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private CanvasGroup _canvasGroup;

        private Coroutine _enableFadeCoroutine;

        public void Init() {
            _canvasGroup = GetComponent<CanvasGroup>();

            foreach (var movableElement in _movableElements) {
                movableElement.Init();
            }
			
            var safeAreaManager = SafeAreaManager.Instance;
            if (safeAreaManager.IsSafeAreaEmpty() == false && _canvasScaler != null) {
                safeAreaManager.OnSaveAreaUpdated.AddListener(OnSafeAreaUpdated);
				OnSafeAreaUpdated(safeAreaManager.SafeAreaOffsets);
            }
        }

        public void DoFade(bool active, float fadeDuration) {
            CoroutineHelper.Stop(ref _enableFadeCoroutine);
            _canvasGroup.alpha = active ? 0f : 1f;   // if we want to activate it => set alpha to 0f
            _canvasGroup.enabled = fadeDuration > 0f;

            if (active && gameObject.activeSelf == false) {
                gameObject.SetActive(true);
            }

            if (fadeDuration < 0.05f) {
                _canvasGroup.alpha = active ? 1f : 0f;
                if (_canvasGroup.enabled) _canvasGroup.enabled = false;
                if (active) {
                    foreach (var movableElement in _movableElements) {
                        movableElement.Appear(1f);
                    }
                }
                else {
                    gameObject.SetActive(false);
                }

                return;
            }
            _enableFadeCoroutine = CoroutineHelper.Run(Fade(active, fadeDuration));
        }

        public void StopTransition() {
            CoroutineHelper.Stop(ref _enableFadeCoroutine);
        }

        private IEnumerator Fade(bool active, float fadeDuration) {
            if (_alphaFadeOnTranstion) {
                float targetAlpha = active ? 1f : 0f;
                float startAlpha = 1f - targetAlpha;

                if (active) {
                    for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / fadeDuration) {
                        _canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, targetAlpha, _alphaAppearCurve.Evaluate(i));
                        foreach (var movableElement in _movableElements) {
                            movableElement.Appear(progress: i);
                        }
                        yield return null;
                    }
                    foreach (var movableElement in _movableElements) {
                        movableElement.Appear(1f);
                    }
                }
                else {
                    for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / fadeDuration) {
                        _canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, targetAlpha, _alphaHideCurve.Evaluate(i));
                        foreach (var movableElement in _movableElements) {
                            movableElement.Hide(progress: i);
                        }
                        yield return null;
                    }
                }


                _canvasGroup.alpha = targetAlpha;
                _canvasGroup.enabled = false;
            }
            else {
                _canvasGroup.alpha = 1f;
                if (active) {
                    for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / fadeDuration) {
                        foreach (var movableElement in _movableElements) {
                            movableElement.Appear(progress: i);
                        }
                        yield return null;
                    }
                    foreach (var movableElement in _movableElements) {
                        movableElement.Appear(1f);
                    }
                }
                else {
                    for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / fadeDuration) {
                        foreach (var movableElement in _movableElements) {
                            movableElement.Hide(progress: i);
                        }
                        yield return null;
                    }
                }
            }

            if (!active) {
                gameObject.SetActive(false);
            }

            _enableFadeCoroutine = null;
        }

        private void OnSafeAreaUpdated(Vector4 newSafeArea) {			
            var xRefCoef = _canvasScaler.referenceResolution.x / Screen.width;
            var yRefCoef = _canvasScaler.referenceResolution.y / Screen.height;

            newSafeArea.x *= xRefCoef;
            newSafeArea.y *= xRefCoef;
            newSafeArea.z *= yRefCoef;
            newSafeArea.w *= yRefCoef;

            foreach (var movableElement in _movableElements) {
                movableElement.RecalculateHidenPosition(newSafeArea);
            }
        }
    }
}