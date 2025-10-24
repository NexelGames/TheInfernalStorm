using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
	[System.Serializable]
    public class LegacyTextWithIconBridge : IText
    {
        [SerializeField] private TextWithIcon _component;

        public LegacyTextWithIconBridge() { }
        public LegacyTextWithIconBridge(TextWithIcon component)
        {
            _component = component;
        }

        public MaskableGraphic Graphic => _component.TextComponent;

        private bool _inited = false;

        public string text
        {
            get => _component.text;
            set
            {
                if (_inited) _component.text = value;
                else
                {
                    _component.Init();
                    _component.text = value;
                    _inited = true;
                }
            }
        }

        public float preferredHeight => _component.TextComponent.preferredWidth;
        public float preferredWidth => _component.TextComponent.preferredHeight;
    }
}
