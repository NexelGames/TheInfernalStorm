using UnityEditor;
using UnityEngine;

namespace Game {
    [CustomPropertyDrawer(typeof(Core.Cost))]
    public class CostPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth - 105, position.height);
            var currencyRect = new Rect(position.x + position.width - 105, position.y, 105, position.height);

            EditorGUI.LabelField(labelRect, label);

            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);
            EditorGUI.PropertyField(currencyRect, property.FindPropertyRelative("Type"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}