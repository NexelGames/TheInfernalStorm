using UnityEngine;

namespace Game.JoystickUI {
    public class DynamicJoysticBehaviour : JoystickBehaviour {
        public DynamicJoysticBehaviour(Joystick joystick) {
            if (joystick.Behaviour != null) {
                joystick.Behaviour.RetrieveData(ref _circleCenter, ref _touchPos);
            }

            _joystick = joystick;
            _transform = joystick.transform;
        }

        public override void OnTouchBegan(Vector2 atPosition) {
            _circleCenter = atPosition;
            _joystick.UpdatePositions(_circleCenter, _circleCenter);
        }

        public override void OnTouchMove(Vector2 position) {
            _touchPos = position;

            Vector2 fingerPosDelta = _touchPos - _circleCenter;
            float fingerPosMagnitude = fingerPosDelta.magnitude;

            if (fingerPosMagnitude < EPSILON) {
                MoveCoef = 0f;
                _joystick.UpdatePositions(_circleCenter, _circleCenter);
                return;
            }

            _fingerDirection = fingerPosDelta.normalized;
            MoveDirection = new Vector3(_fingerDirection.x, 0, _fingerDirection.y);
            MoveCoef = Mathf.Clamp(fingerPosMagnitude / _joystick.JoystickScaledBackgroundRadius, 0, 1);

            if (fingerPosMagnitude > _joystick.JoystickScaledBackgroundRadius) {
                _circleCenter += _fingerDirection * (fingerPosMagnitude - _joystick.JoystickScaledBackgroundRadius);
            }

            _joystick.UpdatePositions(_circleCenter,
                _circleCenter + _fingerDirection * MoveCoef * _joystick.JoystickScaledBackgroundRadius);
        }

        public override void OnTouchRelease() {
            _fingerDirection = Vector3.zero;
            MoveCoef = 0f;
        }
    }
}
