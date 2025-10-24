using UnityEngine;

namespace Game.JoystickUI {
    [CreateAssetMenu(menuName = "Common/Joystick Settings", fileName = "Joystick Settings")]
    public class JoystickSettingsSO : ScriptableObject {
        //[Tooltip("radius of move zone by reference resolution [720x1280 in base project]")]
        //public float JoystickRadius = 150f;
        //[Tooltip("radius of stick sprite by reference resolution [720x1280 in base project]")]
        //public float StickSpriteRadius = 30f;
        [Tooltip("type of behaviour. \nDynamic type moves joystick center to touch direction, Fixed - not")]
        public JoystickBehaviours DefaultJoystickBehaviour = JoystickBehaviours.Fixed;
        [Tooltip("should joystick visuals be rendered on UI")]
        public bool RenderJoystickOnUI = true;
    }
}