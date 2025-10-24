using UnityEngine;

namespace Game.Core {
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        /// <summary>
        /// will be object destroyed after base Awake func?
        /// </summary>
        protected bool _awakeDestroyObj = false;

        protected static T _instance;
        public static T Instance {
            get {
                if (_instance == null) {
                    var component = FindAnyObjectByType(typeof(T), FindObjectsInactive.Include);
                    if (component == null) {
                        GameObject go = new GameObject(typeof(T).ToString());
                        _instance = go.AddComponent<T>();
                    }
                    else {
                        return component as T;
                    }

                    if (_instance == null) {
                        Debug.LogError($"Can't return instance of {typeof(T)}. Something went wrong!");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake() {
            if (_instance == null) {
                _instance = GetComponent(typeof(T)) as T;
            }
            else if (_instance != this) {
                Destroy(gameObject);
                _awakeDestroyObj = true;
                return;
            }
        }

        public static void DeleteInstance() {
            if (_instance.gameObject != null) {
                Destroy(_instance.gameObject);
            }
            _instance = default;
        }
    }
}