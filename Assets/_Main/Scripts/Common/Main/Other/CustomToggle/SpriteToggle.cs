using Alchemy.Inspector;
using UnityEngine;

namespace Game.UI {
    /// <summary>
    /// Sprite toggle with IsOn property. You need to manualy change IsOn in other scripts.
    /// </summary>
	[AddComponentMenu("Common/Toggle/Sprite Toggle")]
    public class SpriteToggle : CustomToggle {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField, Preview]
        protected Sprite _onSprite, _offSprite;

        protected override void UpdateState() {
            _spriteRenderer.sprite = _isOn ? _onSprite : _offSprite;

            if (_spriteRenderer.sprite == null) {
                _spriteRenderer.enabled = false;
            }
            else if (_spriteRenderer.enabled == false) {
                _spriteRenderer.enabled = true;
            }
        }
    }
}