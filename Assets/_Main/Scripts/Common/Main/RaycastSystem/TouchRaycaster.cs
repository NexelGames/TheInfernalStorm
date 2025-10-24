using Alchemy.Inspector;
using UnityEngine;
using Lean.Touch;

namespace Game.TouchRaycasters {
    public abstract class TouchRaycaster : MonoBehaviour {
        [SerializeField] protected LayerMask _layerMask;

        [SerializeField] protected float _raycastMaxDistance = 100f;

        protected Camera _mainCamera;
        [SerializeField, ReadOnly]
        protected bool _isSubscribedToEvents = false;

        /// <summary>
        /// if true, then it will be subscribed to touch events 
        /// and do raycast on events<br>
        /// if false - it will unsubscribe from touch events and do nothing</br>
        /// </summary>
        public bool UseRaycaster {
            get => _isSubscribedToEvents;
            set {
                if (_isSubscribedToEvents && value == false) {
                    UnsubscribeTouchEvents();
                }
                else if (!_isSubscribedToEvents && value == true) {
                    SubscribeEvents();
                }
            }
        }

        private void Start() {
            _mainCamera = Camera.main;
            OnStart();
        }

        protected virtual void SubscribeEvents() {
            if (_isSubscribedToEvents) {
                return;
            }
            LeanTouch.OnFingerDown += OnTouchBegan;
            LeanTouch.OnFingerUp += OnTouchEnded;
            _isSubscribedToEvents = true;
        }

        protected virtual void UnsubscribeTouchEvents() {
            LeanTouch.OnFingerDown -= OnTouchBegan;
            LeanTouch.OnFingerUp -= OnTouchEnded;
            _isSubscribedToEvents = false;
        }

        protected abstract void OnStart();
        protected abstract void OnTouchBegan(LeanFinger finger);
        protected abstract void OnTouchEnded(LeanFinger finger);

        public void SetLayerMask(LayerMask layerMask) {
            this._layerMask = layerMask;
        }

        [Button, DisableInEditMode]
        private void Use() {
            RaycastManager.instance.EnableOnly(this);
        }

        [Button, DisableInEditMode]
        private void DontUse() {
            RaycastManager.instance.Disable(this);
        }

        private void OnDestroy() {
            UnsubscribeTouchEvents();
        }
    }
}