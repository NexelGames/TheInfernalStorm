using Alchemy.Inspector;
using Extensions;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game {
    public class CostButton : MonoBehaviour {
        [HideInInspector] public UnityEvent<bool> OnInteractibleChange = new();
        [HideInInspector] public UnityEvent OnClick = new();

        public Cost Cost => _cost;
		public bool Interactible => _button.interactable;
        public bool IsBlocked => _isBlocked;
		public bool CanBuy => IsResourceEnought();

        [SerializeField] private bool _dontChangeButtonInteractibleState = false;
        [Tooltip("Often controllers set cost by SetCost(),\nbut if you want to use Cost from button inspector toggle it to true")]
        [SerializeField] private bool _selfSetCostDataOnInit = false;
        [SerializeField] private CurrenciesSettingsSO _currenciesSettings;
        [SerializeField] protected Cost _cost;
        [SerializeField] protected Image _costIcon;
        [SerializeField] protected Text _costText;
        [ValidateInput(nameof(CheckTextAlighmentForCenterFunction), "Text alighment must be left or right for correct work!")]
        [SerializeField] private bool _centerTextAndIcon = false;

        private static readonly string ZeroCostString = "Free";

        private Button _button;
        protected bool _isBlocked = false;
        protected bool _wasInteractible = true;
        protected CashManager _cashManager;
        private float[] _initTextAndIconPosX;
        private float _centerTextAndIconOffset;
        private float _offsetDirectionCoef;

        private void Awake() {
            Init();
        }

        public virtual void Init() {
			if (_button != null) return;
			
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick.Invoke);
			
            if (_costIcon != null) {
                _costIcon.sprite = _currenciesSettings[_cost.Type].Icon;

                if (_centerTextAndIcon) {
                    _initTextAndIconPosX = new float[2]{
                        _costText.rectTransform.anchoredPosition.x,
                        _costIcon.rectTransform.anchoredPosition.x,
                    };
                    _offsetDirectionCoef = _initTextAndIconPosX[0] > _initTextAndIconPosX[1] ? -1f : 1f;
                    _centerTextAndIconOffset = (-1f) * _offsetDirectionCoef * _costText.preferredWidth * 0.5f;
                }
            }
            else {
                _centerTextAndIcon = false;
            }

            _cashManager = CashManager.Instance;
			_wasInteractible = _button.interactable;

            if (_selfSetCostDataOnInit) SetCost(_cost);
        }

        private void OnEnable() {
            RefreshInteractableOnCurrencyUpdated(_cashManager.GetAmount(_cost.Type), _cost.Type);
            CashManager.OnCurrencyUpdated.AddListener(RefreshInteractableOnCurrencyUpdated);
        }

        private void OnDisable() {
            CashManager.OnCurrencyUpdated.RemoveListener(RefreshInteractableOnCurrencyUpdated);
        }

        public virtual void SetInteractable(bool isInteractable) {
            if (_isBlocked) return;
            if (isInteractable && IsResourceEnought() == false) return;

            if (_wasInteractible != isInteractable) {
                OnInteractibleChange.Invoke(isInteractable);

                if (_dontChangeButtonInteractibleState == false) {
                    _button.interactable = isInteractable;
                }

                _wasInteractible = isInteractable;
            }
        }

        /// <summary>
        /// Spends cash if enough currency
        /// </summary>
        public bool Buy() {
            if (IsResourceEnought()) {
                _cashManager.SpendAndSave(_cost);
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        [Button]
        private void SetCostTest(Cost cost) => SetCost(cost);
#endif

        public virtual void SetCost(Cost cost) {
            SetCost(cost.Type, cost.Value);
            if (_centerTextAndIcon) CenterTextAndIconPosition();
        }

        public void SetCost(Currencies type, float value) {
            _cost.Type = type;
            _cost.Value = value;

            if (_costIcon != null) _costIcon.sprite = _currenciesSettings[_cost.Type].Icon;
            if (value > 0f) {
                _costText.text = ((int)_cost.Value).FormatIntToString();
            }
            else {
                _costText.text = ZeroCostString;
            }

            SetInteractable(IsResourceEnought());
        }

        public virtual void SetBlock(bool isBlocked) {
            _isBlocked = isBlocked;
            if (_isBlocked == false) SetInteractable(IsResourceEnought());
        }

        protected virtual void RefreshInteractableOnCurrencyUpdated(float cashAmount, Currencies currency) {
            if (currency != Cost.Type) return;

            SetInteractable(cashAmount >= _cost.Value);
        }

        private bool IsResourceEnought() {
            return _cashManager.IsEnough(_cost.Value, _cost.Type);
        }

        private void CenterTextAndIconPosition() {
            float prefferedWidth = _costText.preferredWidth;
            float offset = _offsetDirectionCoef * prefferedWidth * 0.5f + _centerTextAndIconOffset;

            var pos = _costText.rectTransform.anchoredPosition;
            pos.x = _initTextAndIconPosX[0] + offset;
            _costText.rectTransform.anchoredPosition = pos;

            pos = _costIcon.rectTransform.anchoredPosition;
            pos.x = _initTextAndIconPosX[1] + offset;
            _costIcon.rectTransform.anchoredPosition = pos;
        }

        private bool CheckTextAlighmentForCenterFunction(object value) {
            if (_centerTextAndIcon && _costText != null &&
                (_costText.alignment == TextAnchor.UpperCenter || _costText.alignment == TextAnchor.LowerCenter || _costText.alignment == TextAnchor.MiddleCenter)) {
                return false;
            }
            return true;
        }
    }
}