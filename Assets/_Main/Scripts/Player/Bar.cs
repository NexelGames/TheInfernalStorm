using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class Bar : MonoBehaviour {
        [Header("UI")]
        [SerializeField] protected Image _fill;                 // Image → Type = Filled
        [SerializeField] private TextMeshProUGUI _valueText;    // Можно не назначать

        [Header("Value")]
        [Min(0f)][SerializeField] protected float _max = 100f;
        [Min(0f)][SerializeField] protected float _current = 100f;

        [Header("Animation")]
        [Tooltip("Ед/сек скорости анимации значения.")]
        [Min(0.01f)][SerializeField] protected float _unitsPerSecond = 200f;

        [Tooltip("Кривая сглаживания анимации (0..1 по времени).")]
        [SerializeField] protected AnimationCurve _ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Игнорировать Time.timeScale.")]
        [SerializeField] protected bool _useUnscaledTime = false;

        private Coroutine _animRoutine;

        public float Max => _max;
        public float Current => _current;
        public float Percent01 => _max <= 0.0001f ? 0f : Mathf.Clamp01(_current / _max);

        protected virtual void Awake() => UpdateVisuals();

        /// Мгновенно установить current/max.
        public virtual void SetInstant(float current, float max) {
            _max = Mathf.Max(0.0001f, max);
            _current = Mathf.Clamp(current, 0f, _max);
            UpdateVisuals();
        }

        /// Плавно анимировать текущее значение к target.
        public virtual void AnimateTo(float target) {
            target = Mathf.Clamp(target, 0f, _max);
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(AnimateValueRoutine(target));
        }

        /// Прибавить/отнять значение.
        public virtual void Add(float delta, bool animate = true) {
            float target = Mathf.Clamp(_current + delta, 0f, _max);
            if (animate) AnimateTo(target);
            else SetInstant(target, _max);
        }

        /// Изменить Max. Если keepPercent = true — сохранить процент (current = Percent * newMax).
        public virtual void SetMax(float newMax, bool keepPercent = true, bool animate = true) {
            newMax = Mathf.Max(0.0001f, newMax);
            float newCurrent = keepPercent ? Mathf.Clamp01(Percent01) * newMax
                                           : Mathf.Clamp(_current, 0f, newMax);

            _max = newMax;
            if (animate) AnimateTo(newCurrent);
            else SetInstant(newCurrent, _max);
        }

        protected IEnumerator AnimateValueRoutine(float target) {
            float start = _current;
            if (Mathf.Approximately(start, target)) { _animRoutine = null; yield break; }

            float delta = Mathf.Abs(target - start);
            float duration = Mathf.Max(0.0001f, delta / _unitsPerSecond);
            float t = 0f;

            while (t < 1f) {
                float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t = Mathf.Clamp01(t + dt / duration);
                float eased = _ease.Evaluate(t);
                _current = Mathf.Lerp(start, target, eased);
                UpdateVisuals();
                yield return null;
            }

            _current = target;
            UpdateVisuals();
            _animRoutine = null;
        }

        protected void UpdateVisuals() {
            if (_fill) _fill.fillAmount = Percent01;
            if (_valueText) _valueText.text = $"{Mathf.RoundToInt(_current)}/{Mathf.RoundToInt(_max)}";
        }


    }
}