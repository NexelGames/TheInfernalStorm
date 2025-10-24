using Game.EditorTools;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game {
    public class PackageGenerateHelper {
        private static readonly string PackagesFolder = "Packages/";
        private static readonly string[] IgnoreFolders =
        {
            "Assets/_Main/Prefabs/Common",
            "Assets/_Main/Sprites/basic",
            "Assets/URP",
            "Assets/Feel",
        };
        private static readonly string[] BaseProjectCubeFiles =
        {
            "VibroBtn.cs",
            "AnimationScript.cs",
            "Joystick.cs",
        };


        [MenuItem("Assets/Move Referenced Assets To Selected", false, 100)]
        public static void MoveAssetReferences() {
            var selectedAsset = Selection.activeObject;
            if (selectedAsset != null) {
                string selectedAssetPath = AssetDatabase.GetAssetPath(selectedAsset);
                string selectedAssetFolder = Path.GetDirectoryName(selectedAssetPath);

                string[] dependenciesPaths = AssetDatabase.GetDependencies(selectedAssetPath, true);

                foreach (string path in dependenciesPaths) {
                    if (IsUnityBuiltInAsset(path) || IsBaseProjectCubeAsset(path)) {
                        continue;
                    }

                    string assetName = Path.GetFileName(path);
                    string newPath = Path.Combine(selectedAssetFolder, assetName);

                    Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);

                    if (assetType.Equals(typeof(GameObject))) EditorAssetManager.MoveAsset(path, newPath, "Prefabs", refreshDatabase: false);
                    else if (assetType.Equals(typeof(Texture2D))) EditorAssetManager.MoveAsset(path, newPath, "Sprites", refreshDatabase: false);
                    else if (typeof(ScriptableObject).IsAssignableFrom(assetType)) EditorAssetManager.MoveAsset(path, newPath, "Configs", refreshDatabase: false);
                    else if (assetType.Equals(typeof(MonoScript))) EditorAssetManager.MoveAsset(path, newPath, "Scripts", refreshDatabase: false);
                    else EditorAssetManager.MoveAsset(path, newPath, null, refreshDatabase: false);
                }

                AssetDatabase.Refresh();
                Debug.Log("Dependencies moved to: " + selectedAssetFolder);
            }
            else {
                Debug.LogWarning("Please select an asset first.");
            }
        }

        private static bool IsUnityBuiltInAsset(string path) {
            return path.StartsWith(PackagesFolder);
        }

        private static bool IsBaseProjectCubeAsset(string path) {
            return IgnoreFolders.Any(ignorePath => path.StartsWith(ignorePath)) ||
                   BaseProjectCubeFiles.Any(ignoreFile => Path.GetFileName(path).Equals(ignoreFile));
        }
    }
}