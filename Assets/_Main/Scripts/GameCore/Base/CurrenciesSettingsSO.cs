using AYellowpaper.SerializedCollections;
using Game.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    [CreateAssetMenu(menuName = "Currencies Settings", fileName = "Currencies Settings")]
    public class CurrenciesSettingsSO : ScriptableObject {
        [SerializedDictionary("Currency", "Settings")] public SerializedDictionary<Currencies, CurrencySettings> CurrenciesData;

        public CurrencySettings this[Currencies currency] => CurrenciesData[currency];

        public Sprite GetCurrencyIconByType(Currencies type) {
            return CurrenciesData[type].Icon;
        }

        public CurrencySettings GetSettings(Currencies currency) {
            return CurrenciesData[currency];
        }

        public Dictionary<Currencies, float> GenerateStartCurrencies() {
            var currencies = new Dictionary<Currencies, float>(CurrenciesData.Count);
            foreach (var data in CurrenciesData) {
                currencies.Add(data.Key, data.Value.StartValue);
            }
            return currencies;
        }
    }

    [Serializable]
    public class CurrencySettings {
        public float StartValue = 0;
        public Sprite Icon;
    }
}