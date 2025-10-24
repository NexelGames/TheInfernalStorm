using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Adds "Sync" button to LocalizationSync script.
    /// </summary>
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Localization Editor"))
            {
                LocalizationEditorWindow.Open();
            }
        }
    }
}