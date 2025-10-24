using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Bar")]
    public class BarUI : MonoBehaviour {
        public Ease ChangeEase = Ease.InOutSine;

        [SerializeField] protected Image _fillImage;
        
        protected float _targetFillAmount;
        private Tween _fillChangeTween;

        public virtual void Init(float fillAmount) {
            _targetFillAmount = fillAmount;
            _fillImage.fillAmount = fillAmount;
        }

        /// <param name="fillAmount">[0..1]</param>
        public virtual void ChangeFill(float fillAmount, float changeTime = 0f) {
            fillAmount = Mathf.Clamp01(fillAmount);
            _targetFillAmount = fillAmount;
            _fillChangeTween.Stop();

            if (changeTime > 0) {
                _fillChangeTween = Tween.UIFillAmount(_fillImage, fillAmount, changeTime, ChangeEase)
                    .OnComplete(SetFillAsFillAmount);
            }
            else {
                _fillImage.fillAmount = _targetFillAmount;
            }
        }

        private void SetFillAsFillAmount() {
            _fillImage.fillAmount = _targetFillAmount;
        }

        public virtual void StopFillChange(bool reachTarget) {
            if (reachTarget) _fillChangeTween.Complete();
            else _fillChangeTween.Stop();

            _targetFillAmount = _fillImage.fillAmount;
        }
    }
}