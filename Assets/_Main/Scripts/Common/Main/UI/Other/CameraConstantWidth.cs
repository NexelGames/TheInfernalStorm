using UnityEngine;
using Alchemy.Inspector;

namespace Game.Core {
    //using Cinemachine;

    // 1) put this script to camera or it's holder
    // 2) set refResolution if you don't have UIManager manualy in script or make it [Serialized field]
    //    notice that it's core part, if your game was made for 720x1280 resolution -> 720x1280 is your reference resolution
    // 3) tweak WidthOrHeight to meet your needs, but usually WidthOrHeight=0 is the best

    /// <summary>
    /// Keeps constant camera width instead of height
    /// Made from tutorial https://youtu.be/0cmxFjP375Y by Emirald Powder
    /// </summary>
	[AddComponentMenu("Common/Camera/Constant Width")]
    public class CameraConstantWidth : MonoBehaviour {
        [SerializeField]
        private Vector2 _refResolution;

        [Blockquote("Should we change FOV if screen is wider then reference resolution")]
        [SerializeField] private bool _useOnWideScreens = false;

        [Blockquote("Leave 0 to make constant width\n" +
                                   "Or 1 to make constant height (as default), or 0-1 in between")]
        [Range(0f, 1f)] public float WidthOrHeight = 0;

        //private CinemachineVirtualCamera componentCamera;

        private float _targetAspect;

        private float _initialFov;
        private float _horizontalFov = 120f;

        private void Start() {
            //componentCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            _targetAspect = _refResolution.x / _refResolution.y;

            //if (componentCamera) {
            //    if (componentCamera.m_Lens.Orthographic) {
            //        float initialSize = componentCamera.m_Lens.OrthographicSize;
            //        float constantWidthSize = initialSize * (targetAspect / componentCamera.m_Lens.Aspect);

            //        if (constantWidthSize > initialSize) {
            //            componentCamera.m_Lens.OrthographicSize = Mathf.Lerp(constantWidthSize, initialSize, WidthOrHeight);
            //        }
            //    }
            //    else {
            //        initialFov = componentCamera.m_Lens.FieldOfView;
            //        horizontalFov = CalcVerticalFov(initialFov, 1 / targetAspect);

            //        float constantWidthFov = CalcVerticalFov(horizontalFov, componentCamera.m_Lens.Aspect);
            //        if (constantWidthFov > componentCamera.m_Lens.FieldOfView || useOnWideScreens) {
            //            componentCamera.m_Lens.FieldOfView = Mathf.Lerp(constantWidthFov, initialFov, WidthOrHeight);
            //        }
            //    }
            //}
            //else {
            Camera cameraDefaultComponent = GetComponentInChildren<Camera>();

            if (cameraDefaultComponent.orthographic) {
                float initialSize = cameraDefaultComponent.orthographicSize;
                float constantWidthSize = initialSize * (_targetAspect / cameraDefaultComponent.aspect);

                if (constantWidthSize > initialSize) {
                    cameraDefaultComponent.orthographicSize = Mathf.Lerp(constantWidthSize, initialSize, WidthOrHeight);
                }
            }
            else {
                _initialFov = cameraDefaultComponent.fieldOfView;
                _horizontalFov = CalcVerticalFov(_initialFov, 1 / _targetAspect);

                float constantWidthFov = CalcVerticalFov(_horizontalFov, cameraDefaultComponent.aspect);
                if (constantWidthFov > cameraDefaultComponent.fieldOfView || _useOnWideScreens) {
                    cameraDefaultComponent.fieldOfView = Mathf.Lerp(constantWidthFov, _initialFov, WidthOrHeight);
                }
            }
            //}
        }

        private float CalcVerticalFov(float hFovInDeg, float aspectRatio) {
            float hFovInRads = hFovInDeg * Mathf.Deg2Rad;
            float vFovInRads = 2 * Mathf.Atan(Mathf.Tan(hFovInRads / 2) / aspectRatio);
            return vFovInRads * Mathf.Rad2Deg;
        }
    }
}