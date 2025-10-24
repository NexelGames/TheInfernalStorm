using UnityEngine;

namespace Game {
    [System.Serializable]
    public abstract class CustomToggleGeneric {
        [Space(10f), SerializeField] protected bool _isOn = true;

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
            }
        }

        public virtual void Init() {
            UpdateState();
        }

        /// <summary>
        /// Sets opposite state
        /// </summary>
        public void Toggle() {
            IsOn = !_isOn;
        }

        protected abstract void UpdateState();
    }
}