using UnityEngine;

namespace Game.Core
{
    public class DefaultCameraLens : CameraLens
    {
        public float orthographicSize
        {
            get => _camera.orthographicSize;
            set => _camera.orthographicSize = value;
        }

        public float fieldOfView
        {
            get => _camera.fieldOfView;
            set => _camera.fieldOfView = value;
        }

        public bool orthographic
        {
            get => _camera.orthographic;
            set => _camera.orthographic = value;
        }

        public float aspect
        {
            get => _camera.aspect;
            set => _camera.aspect = value;
        }

        private Camera _camera;

        public DefaultCameraLens(Camera camera)
        {
            _camera = camera;
        }
    }
}