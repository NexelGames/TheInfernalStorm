using Game.Core;
using System;
using UnityEditor;
using UnityEngine.UI;

namespace Game {
    [CustomEditor(typeof(ImageFillOutIn))]
    public class ImageFillOutInCustomEditor : Editor {
        private SerializedProperty _fillInOrigin;
        private SerializedProperty _fillOutOrigin;

        private bool _inited = false;

        public override void OnInspectorGUI() {
            if (_inited == false) {
                _fillInOrigin = serializedObject.FindProperty(nameof(_fillInOrigin));
                _fillOutOrigin = serializedObject.FindProperty(nameof(_fillOutOrigin));
                _inited = true;
            }

            DrawDefaultInspector();

            var component = (ImageFillOutIn)target;

            bool wasChanged = false;

            if (component.FillImage == null) {
                return;
            }
            else if (component.FillImage.type != Image.Type.Filled) {
                component.FillImage.type = Image.Type.Filled;
                wasChanged = true;
            }

            int valueInBefore = _fillInOrigin.intValue;
            int valueOutBefore = _fillOutOrigin.intValue;

            switch (component.FillImage.fillMethod) {
                case Image.FillMethod.Horizontal:
                    _fillInOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin In", (Image.OriginHorizontal)valueInBefore));
                    _fillOutOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin Out", (Image.OriginHorizontal)valueOutBefore));
                    break;
                case Image.FillMethod.Vertical:
                    _fillInOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin In", (Image.OriginVertical)valueInBefore));
                    _fillOutOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin Out", (Image.OriginVertical)valueOutBefore));
                    break;
                case Image.FillMethod.Radial90:
                    _fillInOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin In", (Image.Origin90)valueInBefore));
                    _fillOutOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin Out", (Image.Origin90)valueOutBefore));
                    break;
                case Image.FillMethod.Radial180:
                    _fillInOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin In", (Image.Origin180)valueInBefore));
                    _fillOutOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin Out", (Image.Origin180)valueOutBefore));
                    break;
                case Image.FillMethod.Radial360:
                    _fillInOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin In", (Image.Origin360)valueInBefore));
                    _fillOutOrigin.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin Out", (Image.Origin360)valueOutBefore));
                    break;
            }

            wasChanged = valueInBefore != _fillInOrigin.intValue || valueOutBefore != _fillOutOrigin.intValue;

            if (wasChanged) {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}