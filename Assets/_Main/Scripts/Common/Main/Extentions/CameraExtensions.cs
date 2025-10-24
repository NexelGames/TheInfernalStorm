// from https://answers.unity.com/questions/501893/calculating-2d-camera-bounds.html
// and  https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
// with some changes

using UnityEngine;

namespace Extensions {
    public static class CameraExtensions {
        public static Vector2 OrthographicBounds(this Camera camera) {
            float cameraHeight = camera.orthographicSize * 2;
            return new Vector2(cameraHeight * camera.aspect, cameraHeight);
        }

        public static Vector2 OrthographicBounds(this Camera camera, float customAspectRatio) {
            float cameraHeight = camera.orthographicSize * 2;
            return new Vector2(cameraHeight * customAspectRatio, cameraHeight);
        }

        public static Vector2 GetFrustrumBounds(this Camera camera, float distance) {
            float frustumHeight = 2.0f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(frustumHeight * camera.aspect, frustumHeight);
        }
    }
}