using UnityEngine;
using Game.Core;
using Alchemy.Inspector;
using UnityEngine.UI;
using Game.UI;

namespace Game {
    public class CostButtonEnhanced : CostButton {
        [Header("Other")]
        [SerializeField] private bool _coloredText;
        [SerializeField, ShowIf(nameof(_coloredText))] private Color _enoughtTextColor = Color.white;
        [SerializeField, ShowIf(nameof(_coloredText))] private Color _notEnoughtTextColor = Color.red;
        [SerializeField, ShowIf(nameof(_coloredText))] private Text[] _additionalColoredTexts;
        [Space]
		[SerializeField, SerializeReference] private CustomToggleGeneric _interactibleToggle;
		private bool _textColorLastStateEnough = true;

        public override void Init() {
            base.Init();
			
            if (_interactibleToggle != null) {
                _interactibleToggle.Init();
            }

			if (_coloredText) {
                PaintText(_textColorLastStateEnough);
            }
        }

        protected override void RefreshInteractableOnCurrencyUpdated(float cashAmount, Currencies currency) {
			if (currency != Cost.Type) return;
            base.RefreshInteractableOnCurrencyUpdated(cashAmount, currency);
            if (_coloredText) TryPaintText(cashAmount);
        }

        public override void SetInteractable(bool isInteractable) {
            base.SetInteractable(isInteractable);
            if (_interactibleToggle != null) _interactibleToggle.IsOn = isInteractable;
        }

        public override void SetCost(Cost cost) {
            base.SetCost(cost);
            if (_coloredText) TryPaintText(_cashManager.GetAmount(_cost.Type));
        }

        public override void SetBlock(bool isBlocked) {
            if (_interactibleToggle != null) {
                bool interactibleToggleOff = isBlocked;

                if (isBlocked == false && _wasInteractible == false) {
                    interactibleToggleOff = true;
                }

                _interactibleToggle.IsOn = interactibleToggleOff == false;
            }

            base.SetBlock(isBlocked);
        }

        private void TryPaintText(float cash) {
            bool isEnough = cash >= _cost.Value;
            if (_textColorLastStateEnough != isEnough) PaintText(isEnough);
        }

        private void PaintText(bool isEnought) {
            var color = isEnought ? _enoughtTextColor : _notEnoughtTextColor;
            _costText.color = color;
            foreach (var additionalColorText in _additionalColoredTexts) {
                if (additionalColorText != null) {
                    additionalColorText.color = color;
                }
            }
            _textColorLastStateEnough = isEnought;
        }
    }
}