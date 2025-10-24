using Game.UI;
using TMPro.EditorUtilities;
using UnityEditor;

[CustomEditor(typeof(LocalizedTextMeshProUI), editorForChildClasses: true)]
public class LocalizedTextMeshProUIEditor : TMP_EditorPanelUI
{
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