using Alchemy.Inspector;
using Game.Core;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    /// <summary>
    /// Cash text value updater that auto updates on cash change
    /// </summary>
	[AddComponentMenu("Common/UI/Cash Value Updater")]
    public class CashValueUpdater : TextValueUpdater {
        public Currencies Currency => _currency;

        [Space]
        [SerializeField] private CurrenciesSettingsSO _currencySettings;
        [SerializeField] private Currencies _currency = Currencies.Cash;
        [SerializeField] private Image _icon;
        [SerializeField] private bool _usePunchScaleOnChange;
        [SerializeField, ShowIf(nameof(_usePunchScaleOnChange)), Indent] private Transform _targetPunchScale;
        [SerializeField, ShowIf(nameof(_usePunchScaleOnChange)), Indent] private Vector3 _punchForce = new(0.25f, 0.25f, 0f);
        [SerializeField, ShowIf(nameof(_usePunchScaleOnChange)), Indent] private float _punchScaleDuration = 0.15f;

        private Tween _punchScaleTween;
        private float _oldCurrencyValue = -1f;

        protected override void Awake() {
            base.Awake();
            if (_icon) {
                _icon.sprite = _currencySettings.GetCurrencyIconByType(_currency);
            }
        }

        private void OnEnable() {
            var newValue = CashManager.Instance.GetAmount(_currency);
            if (_oldCurrencyValue != newValue) {
                if (_changeDuration == 0f) TweenSetValue(newValue);
                else UpdateTextInstant(newValue);
                _oldCurrencyValue = newValue;
            }
            CashManager.OnCurrencyUpdated.AddListener(UpdateCashUI);
        }

        private void OnDisable() {
            CashManager.OnCurrencyUpdated.RemoveListener(UpdateCashUI);
            _punchScaleTween.Complete();
        }

        public void ChangeCurrency(Currencies currency) {
            _icon.sprite = _currencySettings.GetCurrencyIconByType(currency);
            _currency = currency;
            UpdateTextInstant(CashManager.Instance.GetAmount(_currency));
        }

        private void UpdateCashUI(float cash, Currencies currency) {
            if (_currency != currency) {
                return;
            }

            if (_oldCurrencyValue == cash) {
                return;
            }
            _oldCurrencyValue = cash;

            if (_changeDuration == 0f) TweenSetValue(cash);
            else UpdateTextSmooth(cash);

            if (_usePunchScaleOnChange) {
                _punchScaleTween.Complete();
                _punchScaleTween = Tween.PunchScale(_targetPunchScale, _punchForce, _punchScaleDuration, frequency: 2);
            }
        }
    }
}

