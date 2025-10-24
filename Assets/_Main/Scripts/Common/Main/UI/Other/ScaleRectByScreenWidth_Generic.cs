using UnityEngine;

namespace Game.UI {
	[AddComponentMenu("Common/UI/Scale RectTransform By Screen Width")]
    public class ScaleRectByScreenWidth_Generic : MonoBehaviour {
        [SerializeField, Range(0f, 1f)]
        private float _scaleFactor = 1f;

        [SerializeField]
        private Vector2 _refResolution = new Vector2(1080, 1920);

        [SerializeField] private RectTransform[] _targetRects;
        private Vector3[] _initScales;

        [SerializeField] private bool _alwaysUpdate = false;

        private void Start() {
            _initScales = new Vector3[_targetRects.Length];
            int i = 0;
            foreach (RectTransform rect in _targetRects) {
                _initScales[i] = rect.localScale;
                i++;
            }

            Setup();

            if (!_alwaysUpdate) {
                enabled = false;
            }
        }

        private void Update() {
            Setup();
        }

        private void Setup() {
            float refAspect = _refResolution.x / _refResolution.y;
            float curAspect = (float)Screen.width / Screen.height;
            if (curAspect < refAspect) {
                float scaleCoef = Mathf.Lerp(1f, curAspect / refAspect, _scaleFactor);
                int i = 0;
                foreach (RectTransform rect in _targetRects) {
                    rect.localScale = _initScales[i] * scaleCoef;
                    i++;
                }
            }
        }
    }
}