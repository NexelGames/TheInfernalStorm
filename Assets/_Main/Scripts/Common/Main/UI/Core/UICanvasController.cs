using Alchemy.Inspector;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI {

    /// <summary>
    /// Base class for all canvas controllers
    /// </summary>
    [DisallowMultipleComponent]
    public class UICanvasController : MonoBehaviour {
        public static UnityEvent<UICanvasController> OnCreated = new UnityEvent<UICanvasController>();

        /// <summary>
        /// Is controller replaced under Canvas as child?
        /// </summary>
        public bool IsChildCanvas => _isChildCanvas;

        /// <summary>
        /// UICanvasController that is currently active as child of this canvas. Can be null
        /// </summary>
        public UICanvasController ActiveState => _activeState;

        /// <summary>
        /// For canvas holder/parent it's visibility status <br>
        /// For child canvases - will they be activated on holder/parent and visibility status too </br>
        /// </summary>
        public bool IsActive {
            get => _isActive;
            internal set => _isActive = value;
        }

        /// <summary>
        /// Enables or disables graphic raycaster. Has no effect if no component attached to gameObject
        /// </summary>
        public bool RaycasterEnabled {
            get {
                if (_graphicRaycaster) return _graphicRaycaster.enabled;
                return false;
            }
            set {
                if (_graphicRaycaster) _graphicRaycaster.enabled = value;
            }
        }

        public bool HasTransitionComponent => _transitionComponent != null;

        [Tooltip("Leave it null if it's single canvas.\n" +
            "If it's state-driven -> chose active state by default!")]
        [SerializeField] protected UICanvasController _activeState;

        protected bool _inited = false;
        protected bool _isActive = false;
        protected ICanvasTransition _transitionComponent;
        protected UICanvasController _parentController;
        protected T Parent<T>() where T : UICanvasController => (T)_parentController;
        protected GraphicRaycaster _graphicRaycaster;
        protected bool _isChildCanvas = false;
        private bool _wasCreateEventRaised = false;

        protected virtual void Awake() {
            if (_inited == false) {
                InitReferences();
            }
        }

        /// <summary>
        /// Enables/Disables controller, and also it's child if state-driven and activeState != null <br>
        /// Makes it instantly </br>
        /// </summary>
        /// <param name="active">true - enable, false - disable</param>
        public void EnableCanvas(bool active) {
            if (_inited == false) {
                InitReferences();
            }
            EnableCanvas(active, fadeDuration: 0f);
        }

        /// <summary>
        /// Enables/Disables controller, and also it's child if state-driven and activeState != null <br>
        /// Makes it smoothly if <paramref name="fadeDuration"/> > 0 </br>
        /// </summary>
        /// <param name="active">true - enable, false - disable</param>
        /// <param name="fadeDuration">time to fade canvas in or out (alpha change)</param>
        public virtual void EnableCanvas(bool active, float fadeDuration) {
            if (_inited == false) {
                InitReferences();
            }

            _isActive = active;
            RaycasterEnabled = active;

            if (_transitionComponent != null) {
                _transitionComponent.DoFade(active, fadeDuration);
            }
            else {
                gameObject.SetActive(active);
            }

            if (active) {
                if (_activeState != null && _activeState.IsActive == false) {
                    _activeState.EnableCanvas(true, 0f);
                }
            }
        }

        /// <summary>
        /// Set new state of UICanvasController that is child of canvas where it's called
        /// </summary>
        /// <param name="state">Child state (don't set child of child)</param>
        /// <param name="fadeDuration">Smooth fade state change with value > 0f, or instant</param>
        public virtual void SetState(UICanvasController state, float fadeDuration = 0f) {
            if (state == _activeState) return;

            bool enableNewStateWithDelay = false;
            if (_activeState != null) {
                _activeState.EnableCanvas(active: false, fadeDuration);
                enableNewStateWithDelay = _activeState.HasTransitionComponent;
            }
            _activeState = state;
            if (state == null) return;
            if (enableNewStateWithDelay) {
                CoroutineHelper.WaitForAction(
                    () => state.EnableCanvas(active: true, fadeDuration),
                    fadeDuration);
            }
            else {
                state.EnableCanvas(active: true, fadeDuration);
            }
        }

        internal void StopTransition() {
            if (_transitionComponent != null && _transitionComponent.IsRunning) {
                _transitionComponent.StopTransition();
            }
        }

        /// <summary>
        /// Override it to init references as soon as possible
        /// </summary>
        public virtual void InitReferences() {
            if (_inited) {
                return;
            }

            _isActive = gameObject.activeSelf;
            _graphicRaycaster = GetComponent<GraphicRaycaster>();

            if (transform.parent != null) {
                _parentController = transform.parent.GetComponent<UICanvasController>();
                if (_parentController != null) _isChildCanvas = true;
                else _isChildCanvas = IsInheritedCanvas(transform.parent);
            }

            _transitionComponent = GetComponent<ICanvasTransition>();
            if (_transitionComponent != null) {
                _transitionComponent.Init();
            }

            _inited = true;
        }

        private bool IsInheritedCanvas(Transform parent) {
            while (parent != null) {
                if (parent.GetComponent<Canvas>() != null) {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }

        internal void RaiseOnCreateEvent() {
            if (_wasCreateEventRaised) {
                return;
            }
            OnCreated.Invoke(this);
            _wasCreateEventRaised = true;
        }

        [Button, LabelText("Toggle Enabled"), DisableInEditMode]
        public void ToggleEnabled() {
            EnableCanvas(!_isActive, 0.25f);
        }
    }
}