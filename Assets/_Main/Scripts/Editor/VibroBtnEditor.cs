using Game.UI;
using Lofelt.NiceVibrations;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(VibroBtn))]
public class VibroBtnEditor : ButtonEditor {
    private SerializedProperty _serializedSoundClickProperty;

    protected override void OnEnable() {
        base.OnEnable();
        if (_serializedSoundClickProperty == null) {
            var btn = (VibroBtn)target;
            _serializedSoundClickProperty = serializedObject.FindProperty(nameof(btn.ClickSound));
        }
    }

    public override void OnInspectorGUI() {
        VibroBtn btn = (VibroBtn)target;

        bool changeWas = false;

        var newVibroType = (HapticPatterns.PresetType)EditorGUILayout.EnumPopup("Vibration Type", btn.VibrationType);
        if (btn.VibrationType != newVibroType) {
            btn.VibrationType = newVibroType;
            changeWas = true;
        }
        bool useAnimation = EditorGUILayout.Toggle("Use Animation", btn.UseAnimation);
        if (btn.UseAnimation != useAnimation) {
            btn.UseAnimation = useAnimation;
            changeWas = true;
        }
        bool useMultiImageTint = EditorGUILayout.Toggle("Use Multi Image Tint", btn.UseMultiImageTint);
        if (btn.UseMultiImageTint != useMultiImageTint) {
            btn.UseMultiImageTint = useMultiImageTint;
            changeWas = true;
        }
        bool soundClick = EditorGUILayout.Toggle("Use Sound Click", btn.UseClickSound);
        if (btn.UseClickSound != soundClick) {
            btn.UseClickSound = soundClick;
            changeWas = true;
        }
        if (soundClick) {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_serializedSoundClickProperty, new GUIContent("Sound Click"));
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.Space();


        if (changeWas) {
            EditorUtility.SetDirty(target);
        }

        base.OnInspectorGUI();
    }
}
