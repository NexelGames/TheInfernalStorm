using UnityEngine;
using UnityEngine.UI;

namespace Game {
    [System.Serializable]
    public class ImageSpritesSwapToggleGeneric : CustomToggleGeneric {
        [SerializeField] private Image[] _targetImages;
        [SerializeField] private Sprite[] _targetImageOffSprite;

        private Sprite[] _targetImageOnSprite;

        public override void Init() {
            _targetImageOnSprite = new Sprite[_targetImages.Length];
            for (int i = 0; i < _targetImages.Length; i++) {
                _targetImageOnSprite[i] = _targetImages[i].sprite;
            }
            base.Init();
        }


        protected override void UpdateState() {
            if (_isOn) {
                for (int i = 0; i < _targetImages.Length; i++) {
                    _targetImages[i].sprite = _targetImageOnSprite[i];
                }
            }
            else {
                for (int i = 0; i < _targetImages.Length; i++) {
                    _targetImages[i].sprite = _targetImageOffSprite[i];
                }
            }
        }
    }
}