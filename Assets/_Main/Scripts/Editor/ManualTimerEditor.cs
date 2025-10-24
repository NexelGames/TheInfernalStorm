using Game;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ManualTimer), true)]
public class ManualTimerEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();
        SerializedProperty delay = property.FindPropertyRelative("_delay");
        delay.floatValue = EditorGUI.FloatField(position, label, delay.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            delay.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}
