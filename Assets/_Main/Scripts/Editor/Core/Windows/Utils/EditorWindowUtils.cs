using UnityEditor;
using UnityEngine;

namespace Game.EditorTools {
    public class EditorUtils {
        public static void ArrayField(SerializedProperty property, string label) {
            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        public static void Field(SerializedProperty property, string label) {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
    }
}