using UnityEngine;
using PrimeTween;

namespace Game.UI {

    /// <summary>
    /// Manager for all ingame canvases except loaders
    /// </summary>
    [RequireComponent(typeof(UICanvasContainer))]
    public class UIManager : MonoBehaviour {
        public static UIManager instance;

        private UICanvasContainer _canvasContainer;
        private UICanvasController _activeCanvasController;
        private CanvasGroup _canvasGroup;

        [SerializeField] private Vector2 _refResolution;
        private Vector2 _refAndUserWidthHightCoefs;

        /// <summary>
        /// Reference screen resolution, used to compare it with player's screen
        /// </summary>
        public Vector2 ReferenceResolution => _refResolution;
        /// <summary>
        /// Vector2 where x and y are refWidth/UserScreenWidth and refHeight/UserScreenHight
        /// </summary>
        public Vector2 RefAndUserWidthHightCoefs => _refAndUserWidthHightCoefs;

        public float ReferenceAspectRatio => _refResolution.x / _refResolution.y;
        public float UserAspectRatio => (float)Screen.width / Screen.height;
        public float UserAndRefAspectRatioCoef => UserAspectRatio / ReferenceAspectRatio;

        /// <summary>
        /// Visibility of UI by canvas group, returns true if it's visible.<br>
        /// If not visible then canvasGroup alpha = 0f</br>
        /// </summary>
        public bool IsVisibleUI {
            get => _canvasGroup.alpha == 1f;
            set => _canvasGroup.enabled = !value;
        }

        public UICanvasController ActiveCanvas => _activeCanvasController;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _refAndUserWidthHightCoefs = new Vector2(_refResolution.x / Screen.width, _refResolution.y / Screen.height);

            _canvasContainer = GetComponent<UICanvasContainer>();
            _canvasGroup = GetComponent<CanvasGroup>();

            UICanvasController.OnCreated.AddListener(_canvasContainer.OnCanvasCreated);

#if UNITY_EDITOR
            if (transform.parent != null) transform.parent = null;
#endif

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Call it once when scene changes, container will remove all canvases so UI will be rebuilded by SceneContainer
        /// </summary>
        public void OnSceneChanged() {
            _canvasContainer.OnSceneChanged();
        }

        /// <summary>
        /// Returns target non-custom canvas with casted type
        /// </summary>
        public T GetCanvas<T>() where T : UICanvasController {
            return _canvasContainer.GetController<T>();
        }

        public T GetCanvas<T>(string objectName) where T : UICanvasController {
            return _canvasContainer.GetController<T>(objectName);
        }

        /// <summary>
        /// Smooth switch canvas to new instance <br>
        /// Note that child canvases you must switch in holder/parent canvas by SetState() </br>
        /// </summary>
        /// <param name="fadeOut">fade out time in seconds</param>
        /// <param name="fadeIn">fade in time in seconds</param>
        /// <returns>new canvas controller</returns>
        public void SwitchCanvasTo(UICanvasController canvas, float fadeOut = 0f, float fadeIn = 0f) {
            if (canvas == _activeCanvasController) {
                return;
            }

            if (canvas.IsChildCanvas) {
                Debug.LogError("Not allowed to switch canvas to sub canvas!\n" +
                    "It should be switched by it's holder/parent canvas.", canvas);
                return;
            }

            float delay;
            if (_activeCanvasController && _activeCanvasController.HasTransitionComponent) {
                delay = fadeOut;
            }
            else {
                delay = 0f;
            }

            // disable old
            if (_activeCanvasController != null) {
                if (fadeOut > 0f) {
                    _activeCanvasController.EnableCanvas(false, fadeOut);
                }
                else {
                    _activeCanvasController.EnableCanvas(false);
                }
            }

            _activeCanvasController = canvas;

            if (delay > 0f) {
                Tween.Delay(this, delay, target => target.EnableNewCanvas(fadeIn));
            }
            else {
                EnableNewCanvas(fadeIn);
            }
        }

        private void EnableNewCanvas(float fadeIn) {
            if (_activeCanvasController != null) {
                if (fadeIn > 0f) {
                    _activeCanvasController.EnableCanvas(true, fadeIn);
                }
                else {
                    _activeCanvasController.EnableCanvas(true);
                }
            }
        }

        /// <summary>
        /// Smooth switch canvas to new one by canvas type <br>
        /// Note that child canvases you must switch in holder/parent canvas by SetState() </br>
        /// </summary>
        /// <param name="fadeOut">fade out time in seconds</param>
        /// <param name="fadeIn">fade in time in seconds</param>
        /// <returns>new canvas controller</returns>
        public T SwitchCanvasTo<T>(float fadeOut = 0.25f, float fadeIn = 0.25f)
                                where T : UICanvasController {
            var targetController = _canvasContainer.GetController<T>();

            if (targetController == _activeCanvasController) {
                return targetController;
            }

            if (targetController.IsChildCanvas) {
                Debug.LogError("Not allowed to switch canvas to sub canvas!\n" +
                    "It should be switched by it's holder/parent canvas.", targetController);
                return null;
            }

            float delay;
            if (_activeCanvasController && _activeCanvasController.HasTransitionComponent) {
                delay = fadeOut;
            }
            else {
                delay = 0f;
            }

            // disable old
            if (_activeCanvasController != null) {
                if (fadeOut > 0f) {
                    _activeCanvasController.EnableCanvas(false, fadeOut);
                }
                else {
                    _activeCanvasController.EnableCanvas(false);
                }
            }

            _activeCanvasController = targetController;

            if (delay > 0f) {
                Tween.Delay(this, delay, target => target.EnableNewCanvas(fadeIn));
            }
            else {
                EnableNewCanvas(fadeIn);
            }

            return targetController;
        }

        public void DisableAllCanvases() {
            _canvasContainer.DisableAll();
            _activeCanvasController = null;
        }

        public void DisableActiveCanvas() {
            if (_activeCanvasController != null) {
                _activeCanvasController.EnableCanvas(false);
            }
            _activeCanvasController = null;
        }

        /// <param name="fadeOut">fade out duration in seconds</param>
        public void DisableActiveCanvas(float fadeOut) {
            if (_activeCanvasController != null) {
                if (fadeOut > 0f) {
                    _activeCanvasController.EnableCanvas(false, fadeOut);
                }
                else {
                    _activeCanvasController.EnableCanvas(false);
                }
            }
            _activeCanvasController = null;
        }
    }
}