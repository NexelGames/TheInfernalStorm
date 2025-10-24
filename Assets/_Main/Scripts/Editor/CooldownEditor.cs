using Game.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Cooldown), true)]
[CustomPropertyDrawer(typeof(CooldownUnscaled), true)]
public class CooldownEditor : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        SerializedProperty cooldownTime = property.FindPropertyRelative("_time");
        cooldownTime.floatValue = EditorGUI.FloatField(position, label, cooldownTime.floatValue);
        if (EditorGUI.EndChangeCheck()) {
            cooldownTime.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}
