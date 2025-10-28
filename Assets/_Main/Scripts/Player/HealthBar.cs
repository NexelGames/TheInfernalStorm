using UnityEngine;

namespace Game {
    public class HealthBar : Bar {
        public void TakeDamage(float amount) => AnimateTo(_current - Mathf.Abs(amount));
        public void Heal(float amount) => AnimateTo(_current + Mathf.Abs(amount));

        /// ���������/��������� �������� HP c ����������� �������� (�� ���������) � ���������.
        public void SetMaxHP(float newMax, bool keepPercent = true, bool animate = true)
            => SetMax(newMax, keepPercent, animate);
    }
}