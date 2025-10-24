using UnityEngine;
using UnityEngine.UI;
using Alchemy.Inspector;

namespace Game {
    [System.Serializable]
    public class TextWithIcon {

        [SerializeField] private Text _text;
        [SerializeField] private Image _icon;
        [ValidateInput(nameof(CheckTextAlighmentForCenterFunction), "Text alighment must be left or right for correct work!")]
        [SerializeField] private bool _centerIconWithText;

		public Text TextComponent => _text;

        public string text {
            get => _text.text;
            set {
                _text.text = value;
                if (_centerIconWithText) CenterTextAndIconPosition();
            }
        }

        public Sprite sprite {
            get => _icon.sprite;
            set => _icon.sprite = value;
        }

        private float[] _initTextAndIconPosX;
        private float _centerTextAndIconOffset;
        private float _offsetDirectionCoef;

        public void Init() {
            if (_centerIconWithText) {
                _initTextAndIconPosX = new float[2]{
                    _text.rectTransform.anchoredPosition.x,
                    _icon.rectTransform.anchoredPosition.x,
                };
                _offsetDirectionCoef = _initTextAndIconPosX[0] > _initTextAndIconPosX[1] ? -1f : 1f;
                _centerTextAndIconOffset = (-1f) * _offsetDirectionCoef * _text.preferredWidth * 0.5f;
            }
        }

        private void CenterTextAndIconPosition() {
            float prefferedWidth = _text.preferredWidth;
            float offset = _offsetDirectionCoef * prefferedWidth * 0.5f + _centerTextAndIconOffset;

            var pos = _text.rectTransform.anchoredPosition;
            pos.x = _initTextAndIconPosX[0] + offset;
            _text.rectTransform.anchoredPosition = pos;

            pos = _icon.rectTransform.anchoredPosition;
            pos.x = _initTextAndIconPosX[1] + offset;
            _icon.rectTransform.anchoredPosition = pos;
        }

        private bool CheckTextAlighmentForCenterFunction(object value) {
            if (_centerIconWithText && _text != null &&
                (_text.alignment == TextAnchor.UpperCenter || _text.alignment == TextAnchor.LowerCenter || _text.alignment == TextAnchor.MiddleCenter)) {
                return false;
            }
            return true;
        }
    }
}