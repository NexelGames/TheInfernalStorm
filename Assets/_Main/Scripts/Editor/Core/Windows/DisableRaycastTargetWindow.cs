using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EditorTools {
    public class DisableRaycastTargetWindow : EditorWindow {
        public GameObject[] _objects;
        public bool _removeInChildrenToo;

        SerializedObject so;
        SerializedProperty objectsProperty = null;
        SerializedProperty removeChildrenTooProperty = null;

        [MenuItem("Tools/Windows/Raycast Disable")]
        public static void ShowWindow() {
            var window = GetWindow<DisableRaycastTargetWindow>();
            window.titleContent = new GUIContent("Raycast Disable");
            window.Init();
        }

        public void Init() {
            so = new SerializedObject(this);
            objectsProperty = so.FindProperty(nameof(_objects));
            removeChildrenTooProperty = so.FindProperty(nameof(_removeInChildrenToo));
        }

        public void OnGUI() {
            EditorGUILayout.BeginVertical();
            EditorUtils.ArrayField(objectsProperty, "Object where to remove");
            EditorUtils.Field(removeChildrenTooProperty, "Remove in children");
            // draw button
            if (GUILayout.Button(new GUIContent("Remove"))) {
                RemoveSelected();
            }
            EditorGUILayout.EndVertical();

            so.ApplyModifiedProperties(); // Remember to apply modified properties
        }

        private void RemoveSelected() {
            if (_objects == null || _objects.Length == 0) {
                return;
            }

            foreach (var obj in _objects) {
                Graphic[] graphics;
                if (_removeInChildrenToo) {
                    graphics = obj.GetComponentsInChildren<Graphic>();
                }
                else {
                    graphics = obj.GetComponents<Graphic>();
                }

                foreach(var graphic in graphics) {
                    graphic.raycastTarget = false;
                }
            }
        }
    }
}