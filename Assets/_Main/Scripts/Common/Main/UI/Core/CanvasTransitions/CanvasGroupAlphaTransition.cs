using Game.Core;
using System.Collections;
using UnityEngine;

namespace Game.UI {
    [AddComponentMenu("Common/UI/Canvas Alpha Transition")]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupAlphaTransition : MonoBehaviour, ICanvasTransition {
        public bool IsRunning => _enableFadeCoroutine != null;

        private CanvasGroup _canvasGroup;

        private Coroutine _enableFadeCoroutine;


        public void Init() {
            _canvasGroup = GetComponent<CanvasGroup>();
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
                if (!active) {
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
            float targetAlpha = active ? 1f : 0f;
            float startAlpha = 1f - targetAlpha;

            for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / fadeDuration) {
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, i);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.enabled = false;
            if (!active) {
                gameObject.SetActive(false);
            }

            _enableFadeCoroutine = null;
        }
    }
}