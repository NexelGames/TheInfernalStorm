using UnityEngine;

namespace Game {
    public class UnstableFPS : MonoBehaviour {
        [Tooltip("[0..1] values should be")]
        [SerializeField] private AnimationCurve _fpsCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _changeSpeed = 1f;
        [SerializeField] private int _minFPS = 15;
        [SerializeField] private int _maxFPS = 120;

        private float _progress;

        private void Update() {
            _progress += Time.unscaledDeltaTime * _changeSpeed;
            if (_progress > 1f) _progress %= 1f;

            Application.targetFrameRate = (int)Mathf.Lerp(_minFPS, _maxFPS, _fpsCurve.Evaluate(_progress));
        }
    }
}