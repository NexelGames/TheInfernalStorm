using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    [System.Serializable]
    public class ImageFiller : IFiller {
        [SerializeField] private Image _image;

        public float fillAmount { 
            get => _image.fillAmount; 
            set => _image.fillAmount = value; 
        }
    }

    [System.Serializable]
    public class SlicedImageFiller : IFiller {
        [SerializeField] private SlicedFilledImage _image;

        public float fillAmount {
            get => _image.fillAmount;
            set => _image.fillAmount = value;
        }
    }

    [System.Serializable]
    public class SliderFiller : IFiller {
        [SerializeField] private Slider _slider;

        public float fillAmount {
            get => _slider.value;
            set => _slider.value = value;
        }
    }

    public class FillerUI : MonoBehaviour {
        public bool IsChanging => _fillChangeTween.isAlive;
        public Ease Ease = Ease.InOutSine;

        [SerializeField, SerializeReference] private IFiller _filler;

        private Tween _fillChangeTween;

        public void Init(float fillAmount) {
            _filler.fillAmount = fillAmount;
        }

        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            if (gameObject.activeSelf == false) return;
            if (IsChanging) StopFillChange(false);
            gameObject.SetActive(false);
        }

        /// <param name="fillAmount">[0..1]</param>
        public void ChangeFill(float fillAmount) {
            _fillChangeTween.Stop();
            _filler.fillAmount = fillAmount;
        }

        public Tween ChangeFill(float fillAmount, float changeTime) {
            _fillChangeTween.Stop();
            _fillChangeTween = Tween.Custom(target: this, _filler.fillAmount, fillAmount, changeTime, 
                                            (script, value) => script.TweenUpdateFill(value), ease: Ease);
            return _fillChangeTween;
        }

        private void TweenUpdateFill(float fill) {
            _filler.fillAmount = fill;
        }

        public void StopFillChange(bool reachTarget) {
            if (reachTarget) _fillChangeTween.Complete();
            else _fillChangeTween.Stop();
        }
    }
}

