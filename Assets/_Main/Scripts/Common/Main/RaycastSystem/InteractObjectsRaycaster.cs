using Alchemy.Inspector;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

namespace Game.TouchRaycasters {

    /// <summary>
    /// Interactions with the <see cref="InteractObject"/> 
    /// mostly in <see cref="InteractPoint"/> but can be used anywere else
    /// </summary>
	[AddComponentMenu("Common/Raycasters/Interact Object Raycaster")]
    public class InteractObjectsRaycaster : TouchRaycaster {
        /// <summary>
        /// InteractObject can be null if <see cref="_doInteractOnRaycast"/> set to true
        /// </summary>
        public static UnityEvent<InteractObject> OnInteractibleObjFound;

        [SerializeField] private bool _raycastToTouchPos = true;
        /// <summary>
        /// If true, raycast will go from camera to touch position direction<br>
        /// If false, raycast will go always to <see cref="RaycastViewPortPosition"/></br>
        /// </summary>
        public bool RaycastToTouchPosition {
            get => _raycastToTouchPos;
            set => _raycastToTouchPos = value;
        }
        [SerializeField, HideIf("_raycastToTouchPos")]
        private Vector3 _raycastViewPortPos = new Vector3(0.5f, 0.5f);
        /// <summary>
        /// Normalized view port position that raycast will go from camera to<br>
        /// <b>Only if</b> <see cref="RaycastToTouchPosition"/> is false</br>
        /// </summary>
        public Vector3 RaycastViewPortPosition {
            get => _raycastViewPortPos;
            set {
                _raycastViewPortPos.x = Mathf.Clamp01(value.x);
                _raycastViewPortPos.y = Mathf.Clamp01(value.y);
                _raycastViewPortPos.z = 0;
            }
        }

        private InteractObject _currentInteraction;
        /// <summary>
        /// Current running interaction with InteractObject <br>
        /// If you manualy change InteractObject, then old one will be Stopped</br>
        /// </summary>
        public InteractObject CurrentInteraction {
            get => _currentInteraction;
            set {
                // unsubscribe on touch update event
                LeanTouch.OnFingerUpdate -= CheckForInteractiveObjects;
                _onDragEventSubscribed = false;

                if (_currentInteraction != null) {
                    _currentInteraction.StopInteract();
                }
                _currentInteraction = value;
                if (_currentInteraction != null && _currentInteraction.CanInteract()) {
                    _currentInteraction.DoInteract(() => _currentInteraction = null);
                }
                else if (_currentInteraction != null) {
#if UNITY_EDITOR
                    Debug.Log("CurrentInteraction change failed because we cant interact with it yet");
#endif
                    // if interact object can interact, then
                    _currentInteraction = null;
                }
            }
        }

        [SerializeField, Tooltip("If raycast finds interact object, should it auto start interaction")]
        private bool _doInteractOnRaycast = true;

        [SerializeField]
        private bool _useTouchMoveRaycasts = false;
        /// <summary>
        /// By default it's false. 
        /// If false then raycast will work only on touch down event<br>
        /// If true then raycast will work also on touch move events</br>
        /// </summary>
        public bool UseTouchMoveRaycasts {
            get => _useTouchMoveRaycasts;
            set {
                if (_useTouchMoveRaycasts && value == false && _onDragEventSubscribed) {
                    LeanTouch.OnFingerUpdate -= CheckForInteractiveObjects;
                    _onDragEventSubscribed = false;
                }
                else if (!_useTouchMoveRaycasts && value == true && !_onDragEventSubscribed) {
                    LeanTouch.OnFingerUpdate += CheckForInteractiveObjects;
                    _onDragEventSubscribed = true;
                }
                _useTouchMoveRaycasts = value;
            }
        }

        private bool _onDragEventSubscribed;

        protected override void OnStart() {
            GlobalEventsManager.OnGameOver.AddListener(HandleGameOver);
        }

        protected override void OnTouchBegan(LeanFinger finger) {
            if (finger.Index != 0 && finger.Index != -1) {
                return;
            }

            Ray ray;
            if (_raycastToTouchPos) {
                ray = _mainCamera.ScreenPointToRay(finger.ScreenPosition);
            }
            else {
                ray = _mainCamera.ViewportPointToRay(_raycastViewPortPos);
            }

            Debug.DrawLine(ray.origin, ray.origin + ray.direction * _raycastMaxDistance, Color.red, 1f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _raycastMaxDistance, _layerMask.value)) {
                InteractObject interactiveObj = hit.collider.GetComponent<InteractObject>();
                if (interactiveObj != null && interactiveObj.CanInteract()) {
                    if (_currentInteraction != null && _currentInteraction.IsInProgress) {
                        _currentInteraction.StopInteract();
                    }
                    _currentInteraction = interactiveObj;
                    if (_doInteractOnRaycast) {
                        _currentInteraction.DoInteract(onInteractionStop: () => _currentInteraction = null);
                        OnInteractibleObjFound?.Invoke(_currentInteraction);
                    }
                    else {
                        OnInteractibleObjFound?.Invoke(_currentInteraction);
                    }
                }
            }
            else if (_useTouchMoveRaycasts && !_onDragEventSubscribed) {
                LeanTouch.OnFingerUpdate += CheckForInteractiveObjects;
                _onDragEventSubscribed = true;
            }
        }

        protected override void OnTouchEnded(LeanFinger finger) {
            if (_currentInteraction != null) {
                _currentInteraction.StopInteract();
                _currentInteraction = null;
            }

            if (_onDragEventSubscribed) {
                LeanTouch.OnFingerUpdate -= CheckForInteractiveObjects;
                _onDragEventSubscribed = false;
            }
        }

        private void CheckForInteractiveObjects(LeanFinger finger) {
            Ray ray = _mainCamera.ScreenPointToRay(finger.ScreenPosition);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * _raycastMaxDistance, Color.red, 1f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _raycastMaxDistance, _layerMask.value)) {
                InteractObject interactiveObj = hit.collider.GetComponent<InteractObject>();
                if (interactiveObj != null && interactiveObj.CanInteract()) {
                    _currentInteraction = interactiveObj;
                    if (_doInteractOnRaycast) {
                        _currentInteraction.DoInteract(onInteractionStop: () => _currentInteraction = null);
                        OnInteractibleObjFound?.Invoke(_currentInteraction);
                    }
                    else {
                        OnInteractibleObjFound?.Invoke(_currentInteraction);
                    }

                    // unsubscribe on touch update event
                    LeanTouch.OnFingerUpdate -= CheckForInteractiveObjects;
                    _onDragEventSubscribed = false;
                }
            }
        }

        private void HandleGameOver(GameOverEvent gameOverEvent) {
            if (UseRaycaster) {
                GlobalEventsManager.OnGameOver.RemoveListener(HandleGameOver);
                OnTouchEnded(null);
                UnsubscribeTouchEvents();
            }
        }
    }
}