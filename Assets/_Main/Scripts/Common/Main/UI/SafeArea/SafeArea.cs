using UnityEngine;
using UnityEngine.Events;

// package from Crystal impl, but customized
namespace Game.UI {
    /// <summary>
    /// Safe area implementation for notched mobile devices. Usage:
    ///  (1) Add this component to the top level of any GUI panel. 
    ///  (2) If the panel uses a full screen background image, then create an immediate child and put the component on that instead, with all other elements childed below it.
    ///      This will allow the background image to stretch to the full extents of the screen behind the notch, which looks nicer.
    ///  (3) For other cases that use a mixture of full horizontal and vertical background stripes, use the Conform X & Y controls on separate elements as needed.
    /// </summary>
    public class SafeArea : MonoBehaviour {
        public bool ConformsX => _conformX;
        public bool ConformsY => _conformY;

        private RectTransform _panel;
        [SerializeField] private bool _conformX = true;
        [SerializeField] private bool _conformY = true;

        internal static UnityEvent<SafeArea> _onAnnounce = new UnityEvent<SafeArea>();
        internal static UnityEvent<SafeArea> _onDestroyEvent = new UnityEvent<SafeArea>();

        private void Awake() {
            _panel = GetComponent<RectTransform>();

#if UNITY_EDITOR
            if (_panel == null) {
                Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
                return;
            }
#endif

            _onAnnounce.Invoke(this);
        }

        public void ApplySafeArea(Rect area) {
            // Ignore x-axis?
            if (!_conformX) {
                area.x = 0;
                area.width = Screen.width;
            }

            // Ignore y-axis?
            if (!_conformY) {
                area.y = 0;
                area.height = Screen.height;
            }

            // Check for invalid screen startup state on some Samsung devices (see below)
            if (Screen.width > 0 && Screen.height > 0) {
                // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
                Vector2 anchorMin = area.position;
                Vector2 anchorMax = area.position + area.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
                // See https://forum.unity.com/threads/569236/page-2#post-6199352
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0) {
                    _panel.anchorMin = anchorMin;
                    _panel.anchorMax = anchorMax;
                }
            }
        }

        private void OnDestroy() {
            _onDestroyEvent.Invoke(this);
        }
    }
}
