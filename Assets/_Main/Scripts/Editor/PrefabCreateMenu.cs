using Game.Core;
using Game.JoystickUI;
using Game.TouchRaycasters;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools {
    /// <summary>
    /// Class to quickly add core prefabs from Hieararchy window
    /// </summary>
    public class PrefabCreateMenu {
        private static readonly string[] CorePrefabsSearchPath = new string[] { "Assets/_Main/Prefabs" };
        private const string CorePrefabLable = "l:CorePrefab";

        [MenuItem("GameObject/Core Prefabs/Camera Custom")]
        public static void CustomCamera() => SpawnPrefab<CameraFollower>();

        [MenuItem("GameObject/Core Prefabs/Joystick")]
        public static void Joystick_Prefab() => SpawnPrefab<Joystick>();

        [MenuItem("GameObject/Core Prefabs/Raycast Manager")]
        public static void RaycastManager() => SpawnPrefab<RaycastManager>();

        private static void SpawnPrefab<T>() where T : Component {
            var prefab = Get<T>(AssetDatabase.FindAssets(CorePrefabLable, CorePrefabsSearchPath));
            if (prefab != null) {
                var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (Selection.activeGameObject != null) {
                    obj.transform.SetParent(Selection.activeGameObject.transform, false);
                }
                Undo.RegisterCreatedObjectUndo(obj, $"Created object of {typeof(T)}");
                EditorUtility.SetDirty(obj);
            }
            else {
                Debug.LogError($"Prefab was not found! Type: {typeof(T)}");
            }
        }

        private static GameObject Get<T>(string[] guids) where T : Component {
            if (guids == null) {
                return null;
            }
            foreach (var guid in guids) {
                var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object));
                if (obj is GameObject gameObject && gameObject.GetComponent<T>()) {
                    return gameObject;
                }
            }
            return null;
        }
    }
}