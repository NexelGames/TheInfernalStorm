using Unity.Cinemachine;

namespace Game.Core
{
    public class CinemachineCameraLens : CameraLens
    {
        public float orthographicSize
        {
            get => _camera.Lens.OrthographicSize;
            set => _camera.Lens.OrthographicSize = value;
        }

        public float fieldOfView
        {
            get => _camera.Lens.FieldOfView;
            set => _camera.Lens.FieldOfView = value;
        }

        public bool orthographic
        {
            get => _camera.Lens.Orthographic;
            set => _camera.Lens.ModeOverride = value ? LensSettings.OverrideModes.Orthographic : LensSettings.OverrideModes.Perspective;
        }

        public float aspect
        {
            get => _camera.Lens.Aspect;
            set { }
        }

        private CinemachineCamera _camera;

        public CinemachineCameraLens(CinemachineCamera camera)
        {
            _camera = camera;
        }
    }
}