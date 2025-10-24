using UnityEngine;

namespace Game.UI {
    [System.Serializable]
    public class ScaleRectByScreenWidth_Composed {
        [SerializeField, Range(0f, 1f)]
        private float _scaleFactor = 1f;

        [SerializeField]
        private Vector2 _refResolution = new Vector2(1080, 1920);
        public Vector2 RefResoulution => _refResolution;
        public float RefAspect => _refResolution.x / _refResolution.y;

        public void ChangeScale(RectTransform windowRect) {
            float refAspect = _refResolution.x / _refResolution.y;
            float curAspect = (float)Screen.width / Screen.height;
            if (curAspect < refAspect) {
                float scaleCoef = Mathf.Lerp(1f, curAspect / refAspect, _scaleFactor);
                windowRect.localScale *= scaleCoef;
            }
        }
    }
}

