using UnityEngine;

namespace Game.Core {
    [System.Serializable]
    public abstract class CameraFollowLogic {
        public bool Inited => _cameraFollower != null;
        protected CameraFollower _cameraFollower;

        [field: SerializeField] public bool ControlsHolderRotation { get; set; } = false;

        public virtual void Init(CameraFollower cameraFollower) {
            _cameraFollower = cameraFollower;
        }

        public abstract void OnLateUpdate(Transform followTarget);
        public abstract void OnFocusTransitionToFollow(Transform fromPoint, Vector3 targetPosition, Quaternion targetRotation, 
                                                       float transitionProgress01);
        public abstract void OnFollowTargetTransition(Transform fromPoint, Transform toTarget,
                                                      float transitionPositionProgress01, float transitionRotationProgress01);
    }
}