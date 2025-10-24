#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Game.EditorTools {
    public static class EditorAssetManager {
        /// <param name="paths">for example "Assets/_Main/Prefabs</param>
        /// <returns>Returns all prefab GameObjects by path</returns>
        public static GameObject[] GetPrefabsByAssetPath(string[] paths) {
            string[] assetGUIDs = AssetDatabase.FindAssets("t:prefab", paths);

            List<GameObject> objectsResult = new List<GameObject>();
            foreach (string guid in assetGUIDs) {
                GameObject prefab = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as GameObject;
                if (prefab) {
                    objectsResult.Add(prefab);
                }
            }

            if (objectsResult.Count > 0) {
                return objectsResult.ToArray();
            }
            return null;
        }

        /// <param name="name">file name without extention or filter (name "item" will include objects with name "item_1", "item_2", "itemHello" too)</param>
        /// <param name="paths">for example "Assets/_Main/Configs</param>
        /// <returns>Returns asset by path in project</returns>
        public static T LoadAsset<T>(string name, params string[] paths) where T : Object {
            string[] results = AssetDatabase.FindAssets(name, paths);
            foreach (string result in results) {
                var path = AssetDatabase.GUIDToAssetPath(result);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) {
                    return asset;
                }
            }
            return null;
        }
		
		/// <summary>
        /// Creates prefab from gameObject
        /// </summary>
        /// <param name="path">example: 'Assets/_Main/Prefabs/Levels'</param>
        public static GameObject CreatePrefab(GameObject gameObject, string path, out bool success) {
            CreateFolders(path);
            string localPath = path + "/" + gameObject.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, localPath, out success);
            return prefab;
        }

        /// <returns>Path to folder containing that asset (without asset itself)</returns>
        public static string GetAssetFolder(Object asset) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            return Path.GetDirectoryName(assetPath);
        }

        public static void MoveAsset(Object asset, string newPath, string newPathSubFolder = null, bool refreshDatabase = true) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            MoveAsset(assetPath, newPath, newPathSubFolder, refreshDatabase);
        }

        public static void MoveAsset(string assetPath, string newPath, string newPathSubFolder = null, bool refreshDatabase = true) {
            newPath = CreateSubFolder(newPath, newPathSubFolder);
            AssetDatabase.MoveAsset(assetPath, newPath);
            if (refreshDatabase) AssetDatabase.Refresh();
        }

        private static string CreateSubFolder(string newPath, string newPathSubFolder) {
            if (string.IsNullOrEmpty(newPathSubFolder) == false) {
                var folder = Path.GetDirectoryName(newPath);
                var assetName = Path.GetFileName(newPath);
                newPath = Path.Combine(folder, newPathSubFolder, assetName);
                CreateFolders(newPath);
            }
            return newPath;
        }

        /// <summary>
        /// Destroys prefab file
        /// </summary>
        public static void DestroyPrefab(GameObject prefab) {
            if (prefab == null) return;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            AssetDatabase.DeleteAsset(path);
        }

        /// <param name="path">example: 'Assets/_Main/Prefabs/Levels', must start with Assets</param>
        public static void CreateFolders(string path) {
            if (Path.HasExtension(path)) {
                path = Path.GetDirectoryName(path); // get rid of file in path
            }

            if (AssetDatabase.IsValidFolder(path)) {
                // already created
                return;
            }

            string[] folderNames;
            if (path.Contains('/')) {
                folderNames = path.Split('/');
            }
            else if (path.Contains('\\')) {
                folderNames = path.Split('\\');
            }
            else {
                return;
            }

            string currentPath = "Assets";
            for (int i = 1; i < folderNames.Length; i++) {
                string folderName = folderNames[i];
                currentPath = AssetDatabase.IsValidFolder(currentPath + "/" + folderName)
                    ? currentPath + "/" + folderName
                    : AssetDatabase.CreateFolder(currentPath, folderName);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Creates Scriptable object of T type
        /// </summary>
        /// <typeparam name="T">ScriptableObject inherit</typeparam>
        /// <param name="assetPath">for example "Assets/_Main/ScriptableObjects"</param>
        /// <param name="fileName">name of created file. For example "_SkinLibrary" or "_SkinLibrary.asset"</param>
        /// <param name="autoSaveAndRefreshAssets">Refresh unity after asset created. If false -> call <see cref="SaveAndRefreshAssets"/> when all objs created manualy</param>
        /// <returns></returns>
        public static T CreateScribtableObject<T>(string assetPath, string fileName, 
                                                                 bool autoSaveAndRefreshAssets = true) 
                                                        where T : ScriptableObject {
            T scriptableAsset = ScriptableObject.CreateInstance<T>();
            if (!fileName.Contains(".asset")) {
                fileName += ".asset";
            }
            string path = Path.Combine(assetPath, fileName);
            AssetDatabase.CreateAsset(scriptableAsset, path);
            
            if (autoSaveAndRefreshAssets) {
                SaveAndRefreshAssets();
            }

            return scriptableAsset;
        }

        /// <summary>
        /// Simply shortcut for <see cref="AssetDatabase.SaveAssets"/> and <see cref="AssetDatabase.Refresh"/><br>
        /// Used to save all changes from code with project assets</br>
        /// </summary>
        public static void SaveAndRefreshAssets() {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif