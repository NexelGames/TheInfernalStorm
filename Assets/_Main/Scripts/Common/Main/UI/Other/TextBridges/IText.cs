using UnityEngine.UI;

namespace Game.UI {
    public interface IText {
        public MaskableGraphic Graphic { get; }
        public string text { get; set; }
        public float preferredHeight { get; }
        public float preferredWidth { get; }
    }
}