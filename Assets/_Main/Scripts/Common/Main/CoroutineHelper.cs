using System.Collections;
using UnityEngine;

namespace Game.Core {
    public class CoroutineHelper : MonoBehaviour {
		/// <summary>
        /// Use this check OnDestroy() if you need <br></br>
		/// it's to avoid on app exit exception if you try to access Instance when it destroyed
        /// </summary>
		public static bool IsInstanceExist => _instance != null;
		
        private static CoroutineHelper _instance;
        private static CoroutineHelper Instance {
            get {
                if (_instance == null) {
                    _instance = FindAnyObjectByType<CoroutineHelper>();
                    if (_instance == null) {
                        GameObject go = new GameObject("CoroutineHelper");
                        _instance = go.AddComponent<CoroutineHelper>();
                    }
                }
                return _instance;
            }
        }

        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        public delegate void VoidAction();
        /// <summary>
        /// Execute wait for action coroutine on CoroutineHelper
        /// </summary>
        public static Coroutine WaitForAction(VoidAction voidAction, float delay, bool unscaledTime = true) {
            return Instance.StartCoroutine(WaitForActionCoroutine(voidAction, delay, unscaledTime));
        }

        /// <summary>
        /// Execute wait for action coroutine on MonoBehaviour
        /// </summary>
        public static Coroutine WaitForAction(MonoBehaviour behaviour, VoidAction voidAction, float delay, bool unscaledTime = true) {
            return behaviour.StartCoroutine(WaitForActionCoroutine(voidAction, delay, unscaledTime));
        }

        /// <summary>
        /// Invoke void action next frame
        /// </summary>
        public static Coroutine WaitForFrame(VoidAction voidAction) {
            return Instance.StartCoroutine(WaitForFrameCoroutine(voidAction));
        }

        /// <summary>
        /// Invoke void action next fixed update frame
        /// </summary>
        public static Coroutine WaitForFixedFrame(VoidAction voidAction) {
            return Instance.StartCoroutine(WaitForFixedFrameCoroutine(voidAction));
        }

        /// <summary>
        /// Runs coroutine under CoroutineHelper monobehaviour
        /// </summary>
        public static Coroutine Run(IEnumerator coroutine) {
            return Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Runs coroutine on <paramref name="behaviour"/>
        /// </summary>
        public static Coroutine Run(IEnumerator coroutine, MonoBehaviour behaviour) {
            return behaviour.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stop coroutine that running on CoroutineHelper
        /// </summary>
        public static void Stop(ref Coroutine coroutine) {
            if (coroutine != null) {
                Instance.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        /// <summary>
        /// Stop coroutine that running on target behaviour
        /// </summary>
        public static void Stop(MonoBehaviour behaviour, ref Coroutine coroutine) {
            if (coroutine != null) {
                behaviour.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        public static IEnumerator WaitForActionCoroutine(VoidAction action, float delay, bool unscaledTime) {
            if (delay <= 0) {
                action?.Invoke();
                yield break;
            }

            if (unscaledTime) {
                for (float i = 0f; i < 1f; i += Time.unscaledDeltaTime / delay) {
                    yield return null;
                }
            }
            else {
                for (float i = 0f; i < 1f; i += Time.deltaTime / delay) {
                    yield return null;
                }
            }

            action?.Invoke();
        }

        public static IEnumerator WaitForFrameCoroutine(VoidAction action) {
            yield return null;
            action?.Invoke();
        }

        public static IEnumerator WaitForFixedFrameCoroutine(VoidAction action) {
            yield return new WaitForFixedUpdate();
            action?.Invoke();
        }
    }
}