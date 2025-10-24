using UnityEngine;

namespace Game.JoystickUI {
    public enum JoystickBehaviours {
        Fixed,
        Dynamic,

        None = 99,
    }

    public abstract class JoystickBehaviour {
        public float MoveCoef;
        public Vector3 MoveDirection = Vector3.forward;
        public Vector3 Movement => MoveCoef * MoveDirection;

        protected const float EPSILON = 0.00001f;

        protected Joystick _joystick;
        protected Transform _transform;

        protected Vector2 _circleCenter;
        protected Vector2 _touchPos;
        protected Vector2 _fingerDirection;

        public abstract void OnTouchBegan(Vector2 atPosition);
        public abstract void OnTouchMove(Vector2 currentPosition);
        public abstract void OnTouchRelease();

        public virtual void RetrieveData(ref Vector2 center, ref Vector2 touch) {
            center = _circleCenter;
            touch = _touchPos;
        }
    }
}