using UnityEngine;
using PrimeTween;

namespace Game {
    [System.Serializable]
    public class RectSizeHandler {
        public Vector2 MinSize;
        public Vector2 AddSize;

        private RectTransform _rect;
        private Tween _sizeTween;

        public void Init(RectTransform rect) {
            _rect = rect;
        }

        public void UpdateSize(float preferredWidth, float preferredHeight) {
            Vector2 size = new(
                Mathf.Max(MinSize.x, preferredWidth + AddSize.x),
                Mathf.Max(MinSize.y, preferredHeight + AddSize.y)
            );
            _sizeTween.Stop();
            _rect.sizeDelta = size;
        }

        public Tween UpdateSizeSmooth(float preferredWidth, float preferredHeight, float duration, 
                                      Ease ease = Ease.InOutSine, bool unscaledTime = true) {
            Vector2 size = new(
                Mathf.Max(MinSize.x, preferredWidth + AddSize.x),
                Mathf.Max(MinSize.y, preferredHeight + AddSize.y)
            );

            _sizeTween = Tween.UISizeDelta(_rect, size, duration, ease, useUnscaledTime: unscaledTime);
            return _sizeTween;
        }
    }
}