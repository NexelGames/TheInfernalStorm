using UnityEngine;

namespace Game.Core {
    public class PoolsScene : MonoBehaviour {
        [SerializeField] private PoolManager.PoolSettings[] _pools;

        private void Awake() {
            var poolManager = PoolManager.instance;
            var t = transform;

#if UNITY_EDITOR
            if (poolManager == null) {
                var poolManagerObject = EditorTools.EditorAssetManager.LoadAsset<GameObject>("PoolSystem l:CorePrefab", "Assets/_Main/Prefabs");
                Instantiate(poolManagerObject);
                poolManager = PoolManager.instance;
            }
#endif

            foreach(var pool in _pools) {
                poolManager.CreateScenePool(pool, t);
            }

            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/Pools Scene")]
        private static void CreateObject() {
			var poolsInScene = FindAnyObjectByType<PoolsScene>();
			if (poolsInScene == null) {
                poolsInScene = new GameObject("PoolsScene", typeof(PoolsScene)).GetComponent<PoolsScene>();
			}
            UnityEditor.Selection.activeGameObject = poolsInScene.gameObject;
        }
#endif
    }
}