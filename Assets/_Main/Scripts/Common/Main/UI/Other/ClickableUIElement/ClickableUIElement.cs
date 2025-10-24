using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Simple Button/Default Clickable")]
    public class ClickableUIElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        public UnityEvent onPointerDown;
        public UnityEvent onPointerUp;

        [SerializeField]
        protected bool _locked = false;
        /// <summary>
        /// If true - ignore UI element interactions, and if false don't
        /// </summary>
        public virtual bool Lock {
            get => _locked;
            set {
                if (value == false && _isPressed) {
                    OnPointerUp(null);
                }
                _locked = value;
            }
        }

        protected bool _isPressed = false;

        /// <summary>
        /// You can call this method to trigger btn click manualy<br>
        /// But it will be handled automaticaly by player interactions</br>
        /// </summary>
        public void OnPointerDown(PointerEventData eventData) {
            if (_locked) {
                return;
            }
            _isPressed = true;
            onPointerDown?.Invoke();
        }

        // will not work without IPointerDownHandler
        /// <summary>
        /// You can call this method to simulate touch up logic manualy<br>
        /// But it will be handled automaticaly by player interactions</br>
        /// </summary>
        public void OnPointerUp(PointerEventData eventData) {
            if (_locked) {
                return;
            }
            _isPressed = false;
            onPointerUp?.Invoke();
        }
    }
}