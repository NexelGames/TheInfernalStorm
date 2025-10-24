using UnityEngine;

namespace Game.Core {
    [System.Serializable]
    public class CameraDefaultFollowLogic : CameraFollowLogic {
        /// <summary>
        /// How fast camera holder will follow target position
        /// </summary>
        public float FollowPositionLerp = 10f;
        /// <summary>
        /// How fast camera holder will follow target rotation
        /// </summary>
        public float FollowRotationLerp = 10f;

        private Transform _systemHolder;

        public override void Init(CameraFollower cameraFollower) {
            base.Init(cameraFollower);
            _systemHolder = cameraFollower.HolderTransform;
        }

        public override void OnLateUpdate(Transform followTarget) {
            _systemHolder.position = Vector3.Lerp(_systemHolder.position, followTarget.position, FollowPositionLerp * Time.deltaTime);

            if (ControlsHolderRotation) {
                _systemHolder.rotation = Quaternion.Slerp(_systemHolder.rotation, followTarget.rotation, FollowRotationLerp * Time.deltaTime);
            }
        }

        public override void OnFocusTransitionToFollow(Transform fromPoint, Vector3 targetPosition, Quaternion targetRotation, float transitionProgress01) {
            if (ControlsHolderRotation == false) {
                _systemHolder.position = Vector3.Lerp(
                    _systemHolder.position,
                    Vector3.Lerp(fromPoint.position, targetPosition, transitionProgress01),
                    FollowPositionLerp * Time.deltaTime
                );

                if (targetRotation != _systemHolder.rotation) {
                    _systemHolder.rotation = Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(fromPoint.rotation, targetRotation, transitionProgress01),
                        transitionProgress01
                    );
                }
            }
            else {
                _systemHolder.SetPositionAndRotation(
                    Vector3.Lerp(
                        _systemHolder.position,
                        Vector3.Lerp(fromPoint.position, targetPosition, transitionProgress01),
                        FollowPositionLerp * Time.deltaTime
                    ),
                    Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(fromPoint.rotation, targetRotation, transitionProgress01),
                        FollowRotationLerp * Time.deltaTime
                    )
                );
            }
        }

        public override void OnFollowTargetTransition(Transform fromPoint, Transform toTarget, 
                                                      float transitionPositionProgress01, float transitionRotationProgress01) {
            if (ControlsHolderRotation == false) {
                _systemHolder.position = Vector3.Lerp(
                    _systemHolder.position,
                    Vector3.Lerp(fromPoint.position, toTarget.position, transitionPositionProgress01),
                    FollowPositionLerp * Time.deltaTime
                );
            }
            else {
                _systemHolder.SetPositionAndRotation(
                    Vector3.Lerp(
                        _systemHolder.position,
                        Vector3.Lerp(fromPoint.position, toTarget.position, transitionPositionProgress01),
                        FollowPositionLerp * Time.deltaTime
                    ),
                    Quaternion.Slerp(
                        _systemHolder.rotation,
                        Quaternion.Slerp(fromPoint.rotation, toTarget.rotation, transitionRotationProgress01),
                        FollowRotationLerp * Time.deltaTime
                    )
                );
            }
        }
    }
}