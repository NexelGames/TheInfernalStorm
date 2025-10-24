#if UNITY_EDITOR
using Game.Audio;
using Game.Core;
using Game.UI;
using UnityEngine;

namespace Game {
    public static class EditorScenePrepare {
        public static void Setup() {
            VibroBtn.Editor_ClearEvents(); // avoid multiple events if "Editor Playmode options" enabled in Player Settings

            if (PoolManager.instance == null) {
                var poolManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("PoolSystem l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = GameObject.Instantiate(poolManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            if (UIManager.instance == null) {
                var UIManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("UI l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = GameObject.Instantiate(UIManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            if (AudioManager.IsCreated == false) {
                var audioManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("AudioManager l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = GameObject.Instantiate(audioManagerObject);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            else {
                AudioManager.Instance.SubscribeToButtonSoundClick();
            }
            if (LoadScreenManager.instance == null) {
                var loadScreenManager = EditorTools.EditorAssetManager.LoadAsset<GameObject>("LoaderScreens l:CorePrefab", "Assets/_Main/Prefabs");
                var obj = GameObject.Instantiate(loadScreenManager);
                obj.name = obj.name.Replace("(Clone)", string.Empty);
            }
            var sceneSystem = SceneSystem.Instance; // will create scene system if not exists yet
            AppStart.SetupPrimeTweenConfig();
        }
    }
}
#endif