using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {

    //        / Example of usage \
    //  init data by save:
    //  SaveData data = SaveSystem.Instance.Data;
    //  audioToggle.IsOn = data.audioEnabled;
    //  vibroToggle.IsOn = data.hapticEnabled;
    //
    //  subscribe to OnStateChanged event to update save:
    //  audioToggle.OnStateChanged.AddListener((isOn) => data.audioEnabled = isOn);
    //  vibroToggle.OnStateChanged.AddListener((isOn) => data.hapticEnabled = isOn);

    /// <summary>
    /// Image toggle with IsOn property, works with Button
    /// </summary>
	[AddComponentMenu("Common/Toggle/Image Toggle")]
    [RequireComponent(typeof(Button))]
    public class ImageToggle : CustomToggle {
        [SerializeField]
        private Button _buttonReference;
        public Button Button => _buttonReference;

        /// <summary>
        /// Has no effect if connected with SpriteRenderer
        /// </summary>
        public bool RaycastTarget {
            get => _image.raycastTarget;
            set => _image.raycastTarget = value;
        }

        [SerializeField, Preview]
        protected Sprite _onSprite, _offSprite;

        [SerializeField]
        private Image _image;

        protected override void Awake() {
            base.Awake();

            if (_buttonReference) {
                _buttonReference.onClick.AddListener(Toggle);
            }
        }
		
		public void Set(Sprite offSprite, Sprite onSprite) {
            _offSprite = offSprite;
            _onSprite = onSprite;
            UpdateState();
        }

        protected override void UpdateState() {
            _image.sprite = _isOn ? _onSprite : _offSprite;

            if (_image.sprite == null) {
                _image.enabled = false;
            }
            else if (_image.enabled == false) {
                _image.enabled = true;
            }
        }

        private void OnValidate() {
            if (_buttonReference == null) {
                _buttonReference = GetComponent<Button>();
            }
            if (_image == null) {
                _image = GetComponent<Image>();
            }
        }
    }
}