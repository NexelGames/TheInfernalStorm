using UnityEngine;
using System.Collections.Generic;
using Game.Audio;
using System;

namespace Game.Core {
    public class PoolManager : MonoBehaviour {
        public static PoolManager instance;

        [Serializable]
        public struct PoolSettings {
            public GameObject Prefab;
            [Tooltip("How much objects will be in pool after it's created")]
            public int PoolSize;
            [Tooltip("True - if there is all objects busy, pool will expand and create new objects for reuse.")]
            public bool AutoFillPool;
            [Min(0), Tooltip("Leave 0 to use default value, or set your custom > 0\nIf pool doesn't have enough objects it will create new by this count\n" +
                "Has no effect if AutoFillPool = false")]
            public int AutoFillCount;
        }

        [SerializeField] private PoolSettings[] _dontDestroyOnLoadPools;
        [SerializeField] private PoolSettings _soundPool;

        private Dictionary<int, PoolArrayBuffer<ObjectInstance>> _dontDestroyPoolsDictionary = new Dictionary<int, PoolArrayBuffer<ObjectInstance>>();
        private Dictionary<int, PoolArrayBuffer<ObjectInstance>> _scenePoolsDictionary = new Dictionary<int, PoolArrayBuffer<ObjectInstance>>();

        private int _soundKeyPool;

        private const int AUTO_FILL_UP_ARRAY_ELEMENTS_COUNT = 10;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            gameObject.name = "PoolManager";
#endif

            CreatePools();

            SceneSystem.OnSingleModeSceneStartLoading.AddListener(ClearScenePools);
        }

        private void CreatePools() {
            foreach (var pool in _dontDestroyOnLoadPools) {
                CreateDontDestroyPool(pool);
            }
            CreateSoundPool(_soundPool);
        }

        /// <summary>
        /// Call this method to deactivate all active pool objects (applied only for PoolObject and it's child classes)
        /// </summary>
        public void DeactivateActivePoolObjects() {
            PoolObject.OnDeactivateAllPoolObjects.Invoke();
        }

        public void OnSceneChanged() {
            DeactivateActivePoolObjects();
        }

        private void ClearScenePools() {
            _scenePoolsDictionary.Clear();
        }

        /// <summary>
        /// Creates sound pool for <see cref="AudioManager"/>
        /// </summary>
        internal void CreateSoundPool(PoolSettings poolSettings) {
            int id = poolSettings.Prefab.GetInstanceID();
            _soundKeyPool = id;
            CreateDontDestroyPool(poolSettings);
        }

        public void CreateDontDestroyPool(GameObject prefab, int initialPoolSize, bool autoFill = true, int autoFillCount = AUTO_FILL_UP_ARRAY_ELEMENTS_COUNT) {
            int id = prefab.GetInstanceID();
            if (_dontDestroyPoolsDictionary.ContainsKey(id)) {
                return;
            }

            CreateDontDestroyPool(new PoolSettings() {
                Prefab = prefab,
                PoolSize = initialPoolSize,
                AutoFillPool = autoFill,
                AutoFillCount = autoFillCount,
            });
        }

        public void CreateDontDestroyPool(PoolSettings poolSettings) {
            int id = poolSettings.Prefab.GetInstanceID();
            if (_dontDestroyPoolsDictionary.ContainsKey(id)) {
                return;
            }

            if (poolSettings.AutoFillCount == 0) {
                poolSettings.AutoFillCount = AUTO_FILL_UP_ARRAY_ELEMENTS_COUNT;
            }

            ObjectInstance[] poolArray = new ObjectInstance[poolSettings.PoolSize];
            GameObject poolHolder = new GameObject(poolSettings.Prefab.name + " pool");
            poolHolder.transform.parent = transform;
            var holderTransform = poolHolder.transform;

            for (int i = 0; i < poolSettings.PoolSize; i++) {
                ObjectInstance newObject = new ObjectInstance(Instantiate(poolSettings.Prefab) as GameObject);
                newObject.SetParent(holderTransform);
                poolArray[i] = newObject;
            }

            _dontDestroyPoolsDictionary.Add(id, new PoolArrayBuffer<ObjectInstance>(poolArray, poolSettings, poolHolder.transform));
        }

        /// <summary>
        /// Pool that gives only deactivated objects, if there is no free objects - creates new ones runtime <br>
        /// attached to active scene, so on scene change will be destroyed</br>
        /// </summary>
        /// <param name="sceneTransform">!!! not null, should be transfrom from scene where you want to create pool</param>
        public void CreateScenePool(GameObject prefab, int initialPoolSize, Transform sceneTransform, bool autoFill = true, int autoFillCount = AUTO_FILL_UP_ARRAY_ELEMENTS_COUNT) {
            int id = prefab.GetInstanceID();
            if (_scenePoolsDictionary.ContainsKey(id)) {
                return;
            }

            CreateScenePool(new PoolSettings() {
                Prefab = prefab,
                PoolSize = initialPoolSize,
                AutoFillPool = autoFill,
                AutoFillCount = autoFillCount,
            }, sceneTransform);
        }

        /// <summary>
        /// Pool that gives only deactivated objects, if there is no free objects - creates new ones runtime <br>
        /// attached to active scene, so on scene change will be destroyed</br>
        /// </summary>
        public void CreateScenePool(PoolSettings poolSettings, Transform sceneTransform) {
            int id = poolSettings.Prefab.GetInstanceID();
            if (_scenePoolsDictionary.ContainsKey(id)) {
                return;
            }

#if UNITY_EDITOR
            if (sceneTransform == null) {
                Debug.LogError("You are trying to create scene pool but <sceneTransform> is null, it won't be created in scene!");
            }
#endif

            if (poolSettings.AutoFillCount == 0) {
                poolSettings.AutoFillCount = AUTO_FILL_UP_ARRAY_ELEMENTS_COUNT;
            }

            ObjectInstance[] poolArray = new ObjectInstance[poolSettings.PoolSize];
            GameObject poolHolder = new GameObject(poolSettings.Prefab.name + " scene_pool");
            poolHolder.transform.parent = sceneTransform;
            poolHolder.transform.parent = null;
            var holderTransform = poolHolder.transform;

            for (int i = 0; i < poolSettings.PoolSize; i++) {
                ObjectInstance newObject = new ObjectInstance(Instantiate(poolSettings.Prefab) as GameObject);
                newObject.SetParent(holderTransform);
                poolArray[i] = newObject;
            }

            _scenePoolsDictionary.Add(id, new PoolArrayBuffer<ObjectInstance>(poolArray, poolSettings, poolHolder.transform));
        }

        /// <summary>
        /// Clean ups all objects of prefab from pool and holder itself
        /// </summary>
        /// <param name="prefab">pool prefab</param>
        public void DestroyPool(GameObject prefab) {
            int id = prefab.GetInstanceID();
            if (_scenePoolsDictionary.ContainsKey(id)) {
                _scenePoolsDictionary[id].CleanUp();
                _scenePoolsDictionary.Remove(id);
            }
            else if (_dontDestroyPoolsDictionary.ContainsKey(id)) {
                _dontDestroyPoolsDictionary[id].CleanUp();
                _dontDestroyPoolsDictionary.Remove(id);
            }
#if UNITY_EDITOR
            else {
                Debug.LogError($"You have tried destroy pool that doesn't exist. Prefab = {prefab.name}", prefab);
            }
#endif
        }

        /// <summary>
        /// Generic method for all pool types. <br>
        /// If you know what pool type is used for this component you can use direct method for it</br>
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="prefabComponent">Component from prefab</param>
        /// <returns>attached Component on object that just been reused</returns>
        public T Reuse<T>(T prefabComponent) where T : Component {
            int poolKey = prefabComponent.gameObject.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return ReuseGameObjectScenePool(prefabComponent.gameObject).GetComponent<T>();
            else
                return ReuseGameObjectDontDestroyPool(prefabComponent.gameObject).GetComponent<T>();
        }

        public T Reuse<T>(T prefabComponent, Vector3 position, Quaternion rotation) where T : Component {
            int poolKey = prefabComponent.gameObject.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return ReuseGameObjectScenePool(prefabComponent.gameObject, position, rotation).GetComponent<T>();
            else
                return ReuseGameObjectDontDestroyPool(prefabComponent.gameObject, position, rotation).GetComponent<T>();
        }

        /// <summary>
        /// Generic method for all pool types. <br>
        /// If you know what pool type is used for this component you can use direct method for it </br>
        /// </summary>
        /// <returns>GameObject from pool</returns>
        public GameObject Reuse(GameObject prefab) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return ReuseGameObjectScenePool(prefab);
            else
                return ReuseGameObjectDontDestroyPool(prefab);
        }

        public GameObject Reuse(GameObject prefab, Vector3 position, Quaternion rotation) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return ReuseGameObjectScenePool(prefab, position, rotation);
            else
                return ReuseGameObjectDontDestroyPool(prefab, position, rotation);
        }

        /// <summary>
        /// Generic method for all pool types where gameObjects have PoolObject script. <br>
        /// If you know what pool type is used for this component you can use direct method for it </br>
        /// </summary>
        /// <returns>PoolObject or it's inheritance component on object that just been reused</returns>
        public T ReusePoolObject<T>(GameObject prefab) where T : PoolObject {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return (T)ReusePoolObjectScenePool(prefab);
            else
                return (T)ReusePoolObjectDontDestroyPool(prefab);
        }

        public T ReusePoolObject<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : PoolObject {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return (T)ReusePoolObjectScenePool(prefab, position, rotation);
            else
                return (T)ReusePoolObjectDontDestroyPool(prefab, position, rotation);
        }

        /// <summary>
        /// Generic method for all pool types where gameObjects have PoolObject script. <br>
        /// If you know what pool type is used for this component you can use direct method for it </br>
        /// </summary>
        /// <returns>PoolObject or it's inheritance component on object that just been reused</returns>
        public T ReusePoolObject<T>(T poolObjectPrefabComponent) where T : PoolObject {
            int poolKey = poolObjectPrefabComponent.gameObject.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return (T)ReusePoolObjectScenePool(poolObjectPrefabComponent.gameObject);
            else
                return (T)ReusePoolObjectDontDestroyPool(poolObjectPrefabComponent.gameObject);
        }

        public T ReusePoolObject<T>(T poolObjectPrefabComponent, Vector3 position, Quaternion rotation) where T : PoolObject {
            int poolKey = poolObjectPrefabComponent.gameObject.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey))
                return (T)ReusePoolObjectScenePool(poolObjectPrefabComponent.gameObject, position, rotation);
            else
                return (T)ReusePoolObjectDontDestroyPool(poolObjectPrefabComponent.gameObject, position, rotation);
        }

        /// <summary>
        /// Reuse sound object
        /// </summary>
        public Sound ReuseSound() {
            return (Sound)ReusePoolObjectDontDestroyPool(_dontDestroyPoolsDictionary[_soundKeyPool].PrefabIn);
        }

        /// <summary>
        /// Reuse object from scene pool
        /// </summary>
        /// <returns>object from pool if pool exists or null</returns>
        public PoolObject ReusePoolObjectScenePool(GameObject prefab) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _scenePoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse();
                        return objectInstance.PoolObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance();
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance();
                }

                return buffer.PeakCurrentObjectInstance().PoolObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from scene pool
        /// </summary>
        /// <returns>object from pool if pool exists or null</returns>
        public PoolObject ReusePoolObjectScenePool(GameObject prefab, Vector3 position, Quaternion rotation) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _scenePoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse(position, rotation);
                        return objectInstance.PoolObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }

                return buffer.PeakCurrentObjectInstance().PoolObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from dont destroy pool
        /// </summary>
        /// <returns>object from pool if pool exists or null</returns>
        public PoolObject ReusePoolObjectDontDestroyPool(GameObject prefab) {
            int poolKey = prefab.GetInstanceID();

            if (_dontDestroyPoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _dontDestroyPoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse();
                        return objectInstance.PoolObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance();
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance();
                }

                return buffer.PeakCurrentObjectInstance().PoolObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from dont destroy pool
        /// </summary>
        /// <returns>object from pool if pool exists or null</returns>
        public PoolObject ReusePoolObjectDontDestroyPool(GameObject prefab, Vector3 position, Quaternion rotation) {
            int poolKey = prefab.GetInstanceID();

            if (_dontDestroyPoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _dontDestroyPoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse(position, rotation);
                        return objectInstance.PoolObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }

                return buffer.PeakCurrentObjectInstance().PoolObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from scene pool
        /// </summary>
        public GameObject ReuseGameObjectScenePool(GameObject prefab) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _scenePoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse();
                        return objectInstance.GameObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance();
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance();
                }

                return buffer.PeakCurrentObjectInstance().GameObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from scene pool
        /// </summary>
        public GameObject ReuseGameObjectScenePool(GameObject prefab, Vector3 position, Quaternion rotation) {
            int poolKey = prefab.GetInstanceID();

            if (_scenePoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _scenePoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse(position, rotation);
                        return objectInstance.GameObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }

                return buffer.PeakCurrentObjectInstance().GameObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from dont destroy pool
        /// </summary>
        public GameObject ReuseGameObjectDontDestroyPool(GameObject prefab) {
            int poolKey = prefab.GetInstanceID();

            if (_dontDestroyPoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _dontDestroyPoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse();
                        return objectInstance.GameObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance();
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance();
                }

                return buffer.PeakCurrentObjectInstance().GameObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Reuse object from dont destroy pool
        /// </summary>
        public GameObject ReuseGameObjectDontDestroyPool(GameObject prefab, Vector3 position, Quaternion rotation) {
            int poolKey = prefab.GetInstanceID();

            if (_dontDestroyPoolsDictionary.ContainsKey(poolKey)) {
                var buffer = _dontDestroyPoolsDictionary[poolKey];

                if (buffer.AutoFillBuffer) {
                    var objectInstance = GetFirstFreeObject(buffer);

                    if (objectInstance != null) {
                        objectInstance.Reuse(position, rotation);
                        return objectInstance.GameObject;
                    }

                    // if we reached this -> all objects are busy in pool -> create new objects
                    FillUpArrayBuffer(buffer);

                    buffer.SkipCurrent();
                    while (buffer.IsCurrentObjectBusy()) {
                        buffer.SkipCurrent();
                    }
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }
                else {
                    buffer.SkipCurrent();
                    buffer.ReuseCurrentObjectInstance(position, rotation);
                }

                return buffer.PeakCurrentObjectInstance().GameObject;
            }
            else {
                Debug.LogWarning($"There is no {prefab.name} pool!");
                return null;
            }
        }

        /// <summary>
        /// Is pool of targetPrefab exists
        /// </summary>
        public bool HasPoolOf(GameObject targetPrefab) {
            int poolKey = targetPrefab.GetInstanceID();
            return _scenePoolsDictionary.ContainsKey(poolKey) ||
                   _dontDestroyPoolsDictionary.ContainsKey(poolKey);
        }

        private ObjectInstance GetFirstFreeObject(PoolArrayBuffer<ObjectInstance> buffer) {
            int tryGetFreeObjCount = 0;
            while (tryGetFreeObjCount < buffer.Length) {
                if (buffer.IsCurrentObjectBusy()) {
                    tryGetFreeObjCount++;
                    buffer.SkipCurrent();
                }
                else {
                    return buffer.PeakCurrentObjectInstance();
                }
            }
            return null;
        }

        private void FillUpArrayBuffer(PoolArrayBuffer<ObjectInstance> buffer) {
#if UNITY_EDITOR
            Debug.LogWarning($"[pool] Increased pool of {buffer.PrefabIn.name}", buffer.PrefabIn);
#endif

            ObjectInstance[] addObjsArray = new ObjectInstance[buffer.AutoFillCount];
            Transform holder = buffer.HolderTransform;
            for (int i = 0; i < buffer.AutoFillCount; i++) {
                var objectInstance = new ObjectInstance(Instantiate(buffer.PrefabIn) as GameObject);
                objectInstance.SetParent(holder);
                addObjsArray[i] = objectInstance;
            }
            buffer.Add(addObjsArray);
        }

        public class ObjectInstance {
            public GameObject GameObject => _gameObject;
            public PoolObject PoolObject => _poolObjectScript;
            public bool IsObjectBusy => _hasPoolObjectComponent ? _poolObjectScript.IsPoolObjectUsed :
                                                                  _gameObject.activeSelf;

            private GameObject _gameObject;
            private Transform _transform;
            private bool _hasPoolObjectComponent;
            private PoolObject _poolObjectScript;


            public ObjectInstance(GameObject objectInstance) {
                _gameObject = objectInstance;
                _transform = _gameObject.transform;
                _gameObject.SetActive(false);

                if (_gameObject.GetComponent<PoolObject>()) {
                    _hasPoolObjectComponent = true;
                    _poolObjectScript = _gameObject.GetComponent<PoolObject>();
                }
            }

            public void Reuse(Vector3 position, Quaternion rotation) {
                _transform.SetPositionAndRotation(position, rotation);
                _gameObject.SetActive(true);

                if (_hasPoolObjectComponent) {
                    _poolObjectScript.OnObjectReuse();
                }
            }

            public void Reuse() {
                _gameObject.SetActive(true);
                if (_hasPoolObjectComponent) {
                    _poolObjectScript.OnObjectReuse();
                }
            }

            public void SetParent(Transform parent) {
                _transform.SetParent(parent);
            }

            public void DestroyCompletely() {
                Destroy(_gameObject);
            }
        }
    }
}