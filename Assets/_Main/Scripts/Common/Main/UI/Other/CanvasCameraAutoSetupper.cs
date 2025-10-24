using UnityEngine;

namespace Game.Core {
	[AddComponentMenu("Common/UI/Canvas Camera Auto Setup")]
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraAutoSetupper : MonoBehaviour {
        private Canvas _canvas;

        [SerializeField] private float _canvasPlaneDistance = 3.5f;

        private void Awake() {
            if (_canvas == null) {
                _canvas = GetComponent<Canvas>();
            }
        }

        private void OnEnable() {
            if (_canvas.worldCamera == null) {
                _canvas.worldCamera = Camera.main;
                _canvas.planeDistance = _canvasPlaneDistance;
            }
        }
    }
}