using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools {
    /// <summary>
    /// Class to quickly add core gameObjects from Hieararchy window
    /// </summary>
    public class CustomGameObjectCreateMenu {
        private static readonly string[] GameObjectPrefabTemplates = new string[] { "Assets/_Main/Prefabs" };
        private const string PrefabTemplateLable = "l:GameObjectTemplate";

        private const string CanvasObjectTemplate = "CanvasTemplate";

        [MenuItem("GameObject/UI/Vibro Button by Template")]
        public static void VibroButtonCreate() {
            var btn = CreateGameObject("btnVibroTemplate", "Vibro Button");
            CreateCanvasIfNoYet(btn);
        }

        [MenuItem("GameObject/UI/Text Legacy by Template")]
        public static void TextLegacyButtonCreate() {
            var btn = CreateGameObject("txtLegacyTemplate", "Text Legacy");
            CreateCanvasIfNoYet(btn);
        }

        [MenuItem("GameObject/UI/TMP by Template")]
        public static void TextMeshProButtonCreate() {
            var btn = CreateGameObject("txtMeshProTemplate", "Text");
            CreateCanvasIfNoYet(btn);
        }

        [MenuItem("GameObject/Core Templates/Scene Settuper")]
        public static void SceneSettuperCreate() {
            CreateGameObject("SceneSetupperTemplate", "SceneSettuper");
        }

        private static void CreateCanvasIfNoYet(GameObject elementToCheck) {
            if (elementToCheck.GetComponentInParent<Canvas>(includeInactive: true) == null) {
                var canvas = Object.FindAnyObjectByType<Canvas>()?.gameObject;
                if (canvas == null) {
                    canvas = CreateGameObject(CanvasObjectTemplate, "Canvas", selectObject: false, parentToSelected: false);
                }
                var posBeforeParenting = elementToCheck.GetComponent<RectTransform>().anchoredPosition;
                elementToCheck.transform.SetParent(canvas.transform);
                elementToCheck.GetComponent<RectTransform>().anchoredPosition = posBeforeParenting;
                EditorUtility.SetDirty(canvas);
            }
        }

        private static GameObject CreateGameObject<T>(string endObjectName = "") where T : Component {
            var prefab = Get<T>(AssetDatabase.FindAssets(PrefabTemplateLable, GameObjectPrefabTemplates));
            if (prefab != null) {
                var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                if (endObjectName != string.Empty) obj.name = endObjectName;
                if (Selection.activeGameObject != null) {
                    obj.transform.SetParent(Selection.activeGameObject.transform, false);
                }
                Undo.RegisterCreatedObjectUndo(obj, $"Created {endObjectName} object");
                EditorUtility.SetDirty(obj);
                return obj;
            }
            else {
                Debug.LogError($"Prefab was not found! Type: {typeof(T)}");
            }
            return null;
        }

        private static GameObject CreateGameObject(string templateName, string endObjectName = "", PrefabUnpackMode unpackMode = PrefabUnpackMode.Completely, bool selectObject = true, bool parentToSelected = true) {
            var prefab = Get(AssetDatabase.FindAssets(PrefabTemplateLable, GameObjectPrefabTemplates), templateName);
            if (prefab != null) {
                var obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PrefabUtility.UnpackPrefabInstance(obj, unpackMode, InteractionMode.AutomatedAction);

                if (endObjectName != string.Empty) obj.name = endObjectName;
                if (Selection.activeGameObject != null && parentToSelected) {
                    obj.transform.SetParent(Selection.activeGameObject.transform, false);
                }
                else {
                    var prefabContext = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabContext != null) {
                        obj.transform.SetParent(prefabContext.prefabContentsRoot.transform, false);
                    }
                }
                Undo.RegisterCreatedObjectUndo(obj, $"Created {templateName} object");
                EditorUtility.SetDirty(obj);

                if (selectObject) {
                    Selection.activeObject = obj;
                }

                return obj;
            }
            else {
                Debug.LogError($"Prefab {templateName} was not found!");
            }
            return null;
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

        private static GameObject Get(string[] guids, string objectName) {
            if (guids == null) {
                return null;
            }
            foreach (var guid in guids) {
                var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object));
                if (obj is GameObject gameObject && gameObject.name.Equals(objectName)) {
                    return gameObject;
                }
            }
            return null;
        }
    }
}