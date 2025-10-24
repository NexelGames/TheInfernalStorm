using Alchemy.Inspector;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Game.Core {
    [Serializable]
    public enum Currencies : byte {
        Cash,
        Hard
    }

    [Serializable]
    public class Cost {
        public Currencies Type;
        public float Value;
    }

    public class CashManager : MonoSingleton<CashManager> {
        /// <summary>
        /// arg1 is new cash amount<br>
        /// arg2 is cash delta change</br>
        /// arg3 is what currency to update
        /// </summary>
        public static UnityEvent<float, Currencies> OnCurrencyUpdated = new();

        [UnityEngine.SerializeField] private CurrenciesSettingsSO _currenciesSettings;
        private Dictionary<Currencies, float> _currencies;
        private Currencies[] _allCurrencies;

        private SaveData _save;
        private bool _inited = false;

        protected override void Awake() {
            base.Awake();
            if (_awakeDestroyObj) return;
            DontDestroyOnLoad(gameObject);
            Init();
        }

        public void Init() {
            if (_inited) {
                return;
            }

#if UNITY_EDITOR
            gameObject.name = "CashManager";
            if (_currenciesSettings == null) {
                _currenciesSettings = EditorTools.EditorAssetManager.LoadAsset<CurrenciesSettingsSO>("Currencies Settings", "Assets/_Main/Configs");
            }
#endif

            _save = SaveSystem.Instance.Data;
            if (_save.Currencies == null) {
                _save.Currencies = _currenciesSettings.GenerateStartCurrencies();
            }
            _currencies = new Dictionary<Currencies, float>(_save.Currencies);

            _allCurrencies = (Currencies[])Enum.GetValues(typeof(Currencies));
            var saveCurrencies = _save.Currencies;
            foreach (var currency in _allCurrencies) {
                if (_currencies.ContainsKey(currency) == false) {
                    _currencies.Add(currency, 0);
                }

                if (saveCurrencies.ContainsKey(currency) == false) {
                    saveCurrencies.Add(currency, 0);
                }
            }

            _inited = true;
        }

#if UNITY_EDITOR
        private void Start() {
            if (_inited) {
                foreach (var currency in _currencies) {
                    OnCurrencyUpdated.Invoke(currency.Value, currency.Key);
                }
            }
        }
#endif

        /// <summary>
        /// Set target currency with start value
        /// </summary>
        public void SetCurrencyStartValue(Currencies currency, bool save) {
            float startValue = _currenciesSettings[currency].StartValue;
            _currencies[currency] = startValue;
            if (save) {
                _save.Currencies[currency] = startValue;
            }
            OnCurrencyUpdated.Invoke(startValue, currency);
        }

        /// <summary>
        /// Set target value for currency
        /// </summary>
        public void SetCurrencyValue(Currencies currency, float value, bool save) {
            _currencies[currency] = value;
            if (save) {
                _save.Currencies[currency] = value;
            }
            OnCurrencyUpdated.Invoke(value, currency);
        }

        /// <summary>
        /// Get clean copy of all currencies of the game (with target default value 0 as default)
        /// </summary>
        public Dictionary<Currencies, float> GetAllCurrenciesDictionaryCleanCopy(float defaultValue = 0f) {
            var copy = new Dictionary<Currencies, float>(_allCurrencies.Length);
            foreach (var currency in _allCurrencies) {
                copy.Add(currency, defaultValue);
            }
            return copy;
        }

        /// <returns>Cash manager local target currency value</returns>
        public float GetAmount(Currencies currency) {
            return _currencies[currency];
        }

        /// <summary>
        /// Write all currency values to save data
        /// </summary>
        public void SaveStates() {
            foreach (var currency in _allCurrencies) {
                _save.Currencies[currency] = _currencies[currency];
            }
        }

        /// <summary>
        /// Back all currency values to save data values
        /// </summary>
        public void ResetStates() {
            foreach (var currency in _allCurrencies) {
                if (_currencies[currency] != _save.Currencies[currency]) {
                    _currencies[currency] = _save.Currencies[currency];
                    OnCurrencyUpdated.Invoke(_currencies[currency], currency);
                }
            }
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="AddAndSave(float, Currencies)"/>
        /// </summary>
        public void Add(float amount, Currencies currency) {
            _currencies[currency] += amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], currency);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="AddAndSave(Cost)"/>
        /// </summary>
        public void Add(Cost cost) {
            Add(cost.Value, cost.Type);
        }

        public void AddAndSave(float amount, Currencies currency) {
            _currencies[currency] += amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], currency);
            _save.Currencies[currency] = _currencies[currency];
        }

        public void AddAndSave(Cost cost) {
            AddAndSave(cost.Value, cost.Type);
        }

        /// <summary>
        /// Adds cash by currency, doesn't invoke update event (UI), but changes CashManager currency value
        /// </summary>
        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public void AddWithoutUpdateEvent(float amount, Currencies currency, bool withSave = true) {
            _currencies[currency] += amount;
            if (withSave) _save.Currencies[currency] = _currencies[currency];
        }

        /// <summary>
        /// Adds cash by currency, doesn't invoke update event (UI), but changes CashManager currency value
        /// </summary>
        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public void AddWithoutUpdateEvent(Cost cost, bool withSave = true) {
            AddWithoutUpdateEvent(cost.Value, cost.Type, withSave);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="SpendAndSave(float, Currencies)"/>
        /// </summary>
        public void Spend(float amount, Currencies currency) {
            _currencies[currency] -= amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], currency);
        }

        /// <summary>
        /// Note: it doesnt write to save data, if you need use <see cref="SpendAndSave(Cost)"/>
        /// </summary>
        public void Spend(Cost cost) {
            Spend(cost.Value, cost.Type);
        }

        public void SpendAndSave(float amount, Currencies currency) {
            _currencies[currency] -= amount;
            OnCurrencyUpdated.Invoke(_currencies[currency], currency);
            _save.Currencies[currency] = _currencies[currency];
        }

        public void SpendAndSave(Cost cost) {
            SpendAndSave(cost.Value, cost.Type);
        }

        public bool IsEnough(float forAmount, Currencies currency) {
            return _currencies[currency] >= forAmount;
        }

        public bool IsEnough(Cost cost) {
            return _currencies[cost.Type] >= cost.Value;
        }

        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public bool TrySpend(float amount, Currencies currency, bool withSave = true) {
            if (_currencies[currency] >= amount) {
                _currencies[currency] -= amount;
                OnCurrencyUpdated.Invoke(_currencies[currency], currency);
                if (withSave) {
                    _save.Currencies[currency] = _currencies[currency];
                }
                return true;
            }
            return false;
        }

        /// <param name="withSave">also write transaction to save data so action will be saved</param>
        public bool TrySpend(Cost cost, bool withSave = true) {
            return TrySpend(cost.Value, cost.Type, withSave);
        }

#if UNITY_EDITOR
        /// <summary>
        /// USE IT ONLY FOR TESTS IN EDITOR
        /// </summary>
        public int addOrRemoveCashAmountEDITOR = 500;
        public Currencies editorCurrency = Currencies.Cash;

        [Button, LabelText("Get EDITOR cash")]
        private void AddEDITOR() {
            AddAndSave(addOrRemoveCashAmountEDITOR, editorCurrency);
        }

        [Button, LabelText("Spend EDITOR cash")]
        private void SpendEDITORClamped() {
            SpendAndSave(addOrRemoveCashAmountEDITOR, editorCurrency);
        }

        [Button, LabelText("Delete cash data")]
        private void DeleteEDITORCashData() {
            _currencies = new SaveData().Currencies;

            if (_allCurrencies != null) {
                foreach (var currency in _allCurrencies) {
                    OnCurrencyUpdated.Invoke(_currencies[currency], currency);
                }
            }
        }

        [Button, LabelText("Log local manager currency values")]
        private void LogValues() {
            print(Newtonsoft.Json.JsonConvert.SerializeObject(_currencies));
        }
#endif
    }
}