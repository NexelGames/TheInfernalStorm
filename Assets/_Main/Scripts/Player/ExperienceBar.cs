using PrimeTween;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Game {
    public class ExperienceBar : Bar {
        [Header("Level")]
        [Min(1)] public int Level = 1;

        [Tooltip("Кривая требования XP на текущий уровень. " +
                 "Берётся expCurve.Evaluate(Level) * curveScale. " +
                 "Например: при Level=1 → 100, Level=2 → 150, Level=3 → 225 и т.п.")]
        public AnimationCurve expCurve = AnimationCurve.Linear(1, 100, 10, 1000);

        [Tooltip("Множитель к значению кривой (удобно подстроить масштаб).")]
        public float curveScale = 1f;

        [Header("Level UI (опционально)")]
        [SerializeField] private TextMeshProUGUI _levelText;     // Отдельный текст уровня, если нужен
        [SerializeField] private string _levelPrefix = "Lv. ";   // Префикс для вывода (например, "Ур. ")

        public System.Action<int, float> OnLevelUp; // (newLevel, newMaxXP)

        protected override void Awake() {
            // При старте приведём Max к требуемому для текущего уровня
            RecalculateMaxFromLevel(keepPercent: false, animate: false);
            base.Awake();
            UpdateLevelText();
        }

        /// <summary>
        /// Вручную выставить уровень (например, загрузка сохранения).
        /// </summary>
        public void SetLevel(int newLevel, bool keepPercent = false, bool animate = false) {
            Level = Mathf.Max(1, newLevel);
            RecalculateMaxFromLevel(keepPercent, animate);
            UpdateLevelText();
        }

        /// <summary>
        /// Пересчитать _max по кривой для текущего Level. 
        /// Если keepPercent=true — сохраняет текущий процент заполнения при новом максимуме.
        /// </summary>
        public void RecalculateMaxFromLevel(bool keepPercent, bool animate) {
            float required = Mathf.Max(1f, EvaluateRequiredXP(Level));
            _max = required;

            float newCurrent = keepPercent ? Mathf.Clamp01(Percent01) * _max
                                           : Mathf.Clamp(_current, 0f, _max);

            if (animate) AnimateTo(newCurrent);
            else SetInstant(newCurrent, _max);
        }

        /// <summary>
        /// Добавить опыт. Перелив переносит остаток XP на следующий уровень.
        /// </summary>
        public void AddXP(float amount) {
            if (amount <= 0f) return;
            StartCoroutine(GainWithOverflowRoutine(amount));
        }

        private IEnumerator GainWithOverflowRoutine(float amount) {
            float remaining = amount;

            while (remaining > 0.0001f) {
                float toCap = _max - _current;

                if (toCap <= 0.0001f) {
                    LevelUp();
                    yield return null; // кадр для визуальной фиксации
                    continue;
                }

                float step = Mathf.Min(remaining, toCap);

                // анимируем кусок до ближайшего "упора"
                float start = _current;
                float target = start + step;
                yield return AnimateChunk(start, target);

                _current = target;
                UpdateVisuals();
                remaining -= step;

                if (Mathf.Abs(_current - _max) <= 0.0001f) {
                    LevelUp();
                    yield return null;
                }
            }
        }

        /// <summary> Анимация одного куска заполнения без вмешательства внешних корутин. </summary>
        private IEnumerator AnimateChunk(float start, float target) {
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
        }

        private void LevelUp() {
            // Новый уровень
            Level = Mathf.Max(1, Level + 1);
            UpdateLevelText();

            // Новый максимум по кривой
            _max = Mathf.Max(1f, EvaluateRequiredXP(Level));
            _current = 0f; // после апа бар пустой, остаток уже перенесён в цикле
            UpdateVisuals();

            OnLevelUp?.Invoke(Level, _max);
        }

        private float EvaluateRequiredXP(int lvl) {
            // Безопасно считаем требование по кривой
            float val = expCurve != null ? expCurve.Evaluate(lvl) : 0f;
            return Mathf.Max(0f, val * curveScale);
        }

        private void UpdateLevelText() {
            if (_levelText != null)
                _levelText.text = $"{_levelPrefix}{Level}";
        }
    }
}