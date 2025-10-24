using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI {
    [System.Serializable]
    public enum SafeAreaSides : byte {
        Left,
        Right,
        Top,
        Bottom,
        None,
    }

    [DefaultExecutionOrder(-1)]
    public class SafeAreaManager : MonoBehaviour {
        /// <summary>
        /// arg1 - new <see cref="SafeAreaOffsets"/>, invokes if screen size, safe area or orientation changed
        /// </summary>
        public UnityEvent<Vector4> OnSaveAreaUpdated = new UnityEvent<Vector4>();

        #region Singleton
        private static SafeAreaManager _instance;
        public static SafeAreaManager Instance {
            get {
                if (_instance == null) {
                    GameObject go = new GameObject("SafeAreaManager");
                    _instance = go.AddComponent<SafeAreaManager>();
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Safe area in user screen pixels resolution (convert to reference to panipulate anchorPosition)
        ///  x - left,   y - right,   z - top,   w  - bottom offsets <br>
        /// [0] - left, [1] - right, [2] - top, [3] - bottom offsets </br>
        /// </summary>
        public Vector4 SafeAreaOffsets => _saveAreaOffsets;
        public ScreenOrientation ScreenOrientation => _lastOrientation;
        public Rect SafeAreaRect => _lastSafeArea;

        private Vector4 _saveAreaOffsets;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2Int _lastScreenSize = new Vector2Int(0, 0);
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        [SerializeField] private bool _customAreaForTests;
        [SerializeField, ShowIf(nameof(_customAreaForTests))] private Rect _customSafeArea;

        private List<SafeArea> _safeAreas = new List<SafeArea>();

        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            SafeArea._onAnnounce.AddListener(AddSafeAreaObject);
            SafeArea._onDestroyEvent.AddListener(RemoveSafeAreaObject);
            Refresh();

#if UNITY_EDITOR
            if (transform.parent != null) transform.parent = null;
#endif

            DontDestroyOnLoad(gameObject);
        }

        public bool IsSafeAreaEmpty() {
            return _saveAreaOffsets.x + _saveAreaOffsets.y + _saveAreaOffsets.z + _saveAreaOffsets.w == 0f;
        }

        public float GetSafeAreaSize(SafeAreaSides side) {
            if (side == SafeAreaSides.None) return 0f;
            return _saveAreaOffsets[(int)side];
        }

        private void AddSafeAreaObject(SafeArea safeArea) {
            _safeAreas.Add(safeArea);
            safeArea.ApplySafeArea(_lastSafeArea);
        }

        private void RemoveSafeAreaObject(SafeArea safeArea) {
            _safeAreas.Remove(safeArea);
        }

        public void Update() {
            Refresh();
        }

        private void Refresh() {
            Rect safeArea = Screen.safeArea;

#if UNITY_EDITOR
            if (_customAreaForTests) {
                safeArea = _customSafeArea;
            }
#endif

            if (safeArea != _lastSafeArea
            || Screen.width != _lastScreenSize.x
            || Screen.height != _lastScreenSize.y
            || Screen.orientation != _lastOrientation) {
                // Fix for having auto-rotate off and manually forcing a screen orientation.
                // See https://forum.unity.com/threads/569236/#post-4473253 and https://forum.unity.com/threads/569236/page-2#post-5166467
                _lastScreenSize.x = Screen.width;
                _lastScreenSize.y = Screen.height;
                _lastOrientation = Screen.orientation;
                _lastSafeArea = safeArea;

                _saveAreaOffsets = new Vector4(_lastSafeArea.x,
                                               _lastScreenSize.x - (_lastSafeArea.x + _lastSafeArea.width),
                                               _lastScreenSize.y - (_lastSafeArea.y + _lastSafeArea.height),
                                               _lastSafeArea.y);

                ApplySafeArea();
                OnSaveAreaUpdated.Invoke(_saveAreaOffsets);
            }
        }

        private void ApplySafeArea() {
            foreach (var safeArea in _safeAreas) {
                safeArea.ApplySafeArea(_lastSafeArea);
            }
        }
    }
}