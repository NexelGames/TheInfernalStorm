using UnityEngine;
using UnityEngine.Events;

namespace Game.UI {
    /// <summary>
    /// Base class for Custom toggles
    /// </summary>
    public abstract class CustomToggle : MonoBehaviour {
        /// <summary>
        /// arg1 - changed isOn bool value
        /// </summary>
        public UnityEvent<bool> OnStateChanged = new UnityEvent<bool>();

        [Space(10f), SerializeField]
        protected bool _isOn = true;

        /// <summary>
        /// You can change toggle state IsOn = true/false
        /// </summary>
        public bool IsOn {
            get => _isOn;
            set {
                if (_isOn == value) {
                    return;
                }

                _isOn = value;
                UpdateState();
                OnStateChanged?.Invoke(_isOn);
            }
        }

        protected virtual void Awake() {
            UpdateState();
        }

        /// <summary>
        /// Sets opposite state
        /// </summary>
        public void Toggle() {
            IsOn = !_isOn;
        }
		
		public void SetValue(bool isOn, bool invokeChangedEvent = false) {
            if (_isOn == isOn) {
                return;
            }
            _isOn = isOn;
            UpdateState();

            if (invokeChangedEvent) {
                OnStateChanged?.Invoke(_isOn);
            }
        }

        protected abstract void UpdateState();
    }
}