using Game.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(LocalizedText), editorForChildClasses: true)]
public class LocalizedTextEditor : TextEditor {
    SerializedProperty _localizedKeyProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (_localizedKeyProperty == null) _localizedKeyProperty = serializedObject.FindProperty("_localizedKey");
    }

    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        _localizedKeyProperty.stringValue = EditorGUILayout.TextField("Localized Key", _localizedKeyProperty.stringValue);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        base.OnInspectorGUI();
    }
}