using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Simple Button/Colored Clickable")]
    public class ColoredClickableUIElement : ClickableUIElement {
        [SerializeField] private Graphic[] _targetGraphics;

        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _unpressedColor = Color.white;
        [SerializeField] private Color _lockedColor = Color.white;

        private Color _activeColor = Color.white;

        public override bool Lock {
            get => _locked;
            set {
                base.Lock = value;
                UpdateVisuals();
            }
        }

        private void Awake() {
            UpdateVisuals();
            onPointerDown.AddListener(() => UpdateColor(_pressedColor));
            onPointerUp.AddListener(UpdateVisuals);
        }

        private void UpdateVisuals() {
            if (_locked) {
                UpdateColor(_lockedColor);
            }
            else {
                UpdateColor(_unpressedColor);
            }
        }

        private void UpdateColor(Color clr) {
            if (clr == _activeColor) {
                return;
            }

            foreach (var graphic in _targetGraphics) {
                graphic.color = clr;
            }
            _activeColor = clr;
        }
    }
}