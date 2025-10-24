using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using Alchemy.Inspector;
using PrimeTween;
using System;

namespace Game.UI {

    [AddComponentMenu("Common/UI/Text Value Updater")]
    public class TextValueUpdater : MonoBehaviour {
        public enum UpdateTextStrategies : byte {
            ValueAsItIs,
            ValueFormat,
            Value1k1m1b,
        }
        
        public float CurrentValue => _currentValue;
        public float ChangeDuration => _changeDuration;
        /// <summary>
        /// Interface from what you can get Graphic and convert it to TMP or Text
        /// </summary>
        public IText TextBridge => _textBridge;

        [SerializeField] private UpdateTextStrategies _updateTextStrategy;
        [ShowIf(nameof(NeedShowValueFormatField)), Tooltip("For example '0' -> only int values. '0.0' -> float values with 1 digit after dot")]
        public string ValueFormat = "0.00";

        [Space]
        [SerializeField] private bool _updateInUnscaledTime = true;
        [SerializeField] protected float _changeDuration = 2f;

        [SerializeField, SerializeReference, 
         Tooltip("You can leave it null and it will try initialize runtime instead"), 
         OnValueChanged(nameof(OnTextBridgeSetEditor))] 
        protected IText _textBridge;

        protected float _currentValue = 0f;
        protected Tween _updateTween;
        protected Action _updateTextStrategyFunc;

        protected virtual void Awake() {
            if (_textBridge == null) {
                if (TryGetComponent(out Text legacyText)) _textBridge = new LegacyTextBridge(legacyText);
                else if (TryGetComponent(out TMP_Text textMeshPro)) _textBridge = new TMPTextBridge(textMeshPro);
                else Debug.LogError("[TextValueUpdater] There is no text component attached to gameObject or it's not supported", gameObject);
            }

            _updateTextStrategyFunc = _updateTextStrategy switch {
                UpdateTextStrategies.ValueAsItIs => UpdateTextAsItIs,
                UpdateTextStrategies.ValueFormat => UpdateTextFormat,
                UpdateTextStrategies.Value1k1m1b => UpdateText1k1m1b,
                _ => UpdateTextAsItIs,
            };
        }

        /// <summary>
        /// Updates text
        /// </summary>
        /// <param name="targetValue">future text value</param>
        /// <param name="duration">leave < 0f to use value from inspector</param>
        /// <param name="completeOldIfRunning">true -> set current value to target (if smooth fill)<br>
        /// </br>false -> keep smooth text change from current value</param>
        public void UpdateTextSmooth(float targetValue, float duration = -1f, bool completeOldIfRunning = false) {
            StopUpdate(completeOldIfRunning);
            _updateTween = Tween.Custom(this, _currentValue, targetValue, duration >= 0 ? duration : _changeDuration, 
                                        (target, newValue) => target.TweenSetValue(newValue), useUnscaledTime: _updateInUnscaledTime)
                                .OnComplete(target: this, target => target.TweenSetValue(targetValue));
        }

        /// <summary>
        /// Updates text instantly, stops running tween
        /// </summary>
        public void UpdateTextInstant(float targetValue) {
            StopUpdate(false);
            _currentValue = targetValue;
            _updateTextStrategyFunc.Invoke();
        }

        /// <summary>
        /// Stops update text if running
        /// </summary>
        /// <param name="complete">should value snap to target if running</param>
        public void StopUpdate(bool complete) {
            if (complete) _updateTween.Complete();
            else _updateTween.Stop();
        }

        private void UpdateTextAsItIs() {
            _textBridge.text = _currentValue.ToString();
        }
        private void UpdateTextFormat() {
            _textBridge.text = _currentValue.ToString(ValueFormat);
        }
        private void UpdateText1k1m1b() {
            _textBridge.text = ((int)_currentValue).FormatIntToString(ValueFormat);
        }

        private float TweenGetValue() => _currentValue;
        /// <summary>
        /// You can use this to bypass tween kill check when updating value (<see cref="_changeDuration"/> must be always 0f) for optimization<br>
        /// This method with the case above, or use <see cref="UpdateTextSmooth(float, float, bool)"/> instead</br>
        /// </summary>
        protected void TweenSetValue(float value) {
            _currentValue = value;
            _updateTextStrategyFunc.Invoke();
        }

        private bool NeedShowValueFormatField() => _updateTextStrategy.Equals(UpdateTextStrategies.ValueFormat) || _updateTextStrategy.Equals(UpdateTextStrategies.Value1k1m1b);
        private void OnTextBridgeSetEditor(IText newValue) {
            if (newValue == null) {
                return;
            }

            switch (newValue) {
                case LegacyTextBridge:
                    if (TryGetComponent(out Text legacyText)) _textBridge = new LegacyTextBridge(legacyText);
                    break;
                case TMPTextBridge:
                    if (TryGetComponent(out TMP_Text textMeshPro)) _textBridge = new TMPTextBridge(textMeshPro);
                    break;
            }
        }
    }
}