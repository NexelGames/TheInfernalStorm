using Alchemy.Inspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {
    /// <summary>
    /// Contains all types of existing load screens
    /// </summary>
    [Serializable]
    public enum LoadScreens {
        None = -1,
        //write below new load screen types/behaviours
        FadeInOut,
        ImageFillInOut,
        CutoutCube,
    }

    /// <summary>
    /// Manager of all load screens<br>
    /// Use it to show load screen</br>
    /// </summary>
    public class LoadScreenManager : MonoBehaviour {
        public static LoadScreenManager instance;

        /// <summary>
        /// Is load screen manager active
        /// </summary>
        public static bool IsActive { private set; get; }

        [SerializeField, Blockquote("Each element screen Type should be unique")]
        private LoadScreen[] _loadScreenPrefabs;
        private LoadScreen _activeLoadScreen;

        /// <summary>
        /// Use this function to setup LoadScreen object before it will show/hide <br><br>
        /// </br>
        /// <b>Don't use LoadScreen.Show/Hide methods, you should call only LoadScreenManager for this!</b></br>
        /// </summary>
        public static LoadScreen GetActiveScreen() {
            if (instance == null) {
                return null;
            }
            return instance._activeLoadScreen;
        }

        /// <returns>Undefined state if no screen shown</returns>
        public static LoadScreenState GetActiveScreenState() {
            if (instance == null) return LoadScreenState.Undefined;
            return instance.GetActiveState();
        }

        /// <summary>
        /// Shows load screen
        /// </summary>
        /// <param name="type">Type of load screen</param>
        /// <param name="callback">callback on shown</param>
        public static LoadScreen ShowLoadScreen(LoadScreens type, UnityAction callback = null) {
            if (instance == null) {
                callback?.Invoke();
                Debug.LogWarning("LoadScreenManager doesn't exist, can't perform show. Callback was executed");
                return null;
            }
            instance.Show(type, callback);
            return instance._activeLoadScreen;
        }

        /// <summary>
        /// Shows load screen
        /// </summary>
        /// <param name="loadScreenInstance">load screen instance that you got by <see cref="CreateInstance(LoadScreen)"/></param>
        /// <param name="callback">callback on shown</param>
        public static void ShowLoadScreen(LoadScreen loadScreenInstance, UnityAction callback = null) {
            if (instance == null) {
                callback?.Invoke();
                Debug.LogWarning("LoadScreenManager doesn't exist, can't perform show. Callback was executed");
                return;
            }
            instance.Show(loadScreenInstance, callback);
        }

        /// <summary>
        /// Hides active load screen, if there if no screens callback will be invoked immediately <br>
        /// Note that if you created hide loaded screen earlier you need to call <see cref="Hide(UnityAction, LoadScreen)"/></br>
        /// </summary>
        /// <param name="targetHideScreenTransition">Leave None to keep active screen hide transition, or set another</param>
        public static LoadScreen HideLoadScreen(UnityAction callback = null, LoadScreens targetHideScreenTransition = LoadScreens.None) {
            if (instance == null) {
                callback?.Invoke();
                Debug.LogWarning("LoadScreenManager doesn't exist, can't perform hide. Callback was executed");
                return null;
            }
            instance.Hide(callback, targetHideScreenTransition);
            return instance._activeLoadScreen;
        }

        /// <summary>
        /// Hides active load screen, if there if no screens callback will be invoked immediately
        /// </summary>
        /// <param name="targetHideScreenTransitionInstance">Must be load screen that you got by <see cref="CreateLoadScreen(LoadScreens)"/> or <see cref="Show(LoadScreen, UnityAction)"/></param>
        public static LoadScreen HideLoadScreen(LoadScreen targetHideScreenTransitionInstance, UnityAction callback = null) {
            if (instance == null) {
                callback?.Invoke();
                Debug.LogWarning("LoadScreenManager doesn't exist, can't perform hide. Callback was executed");
                return null;
            }
            instance.Hide(callback, targetHideScreenTransitionInstance);
            return instance._activeLoadScreen;
        }

        /// <summary>
        /// Use this only to pass for <see cref="Show(LoadScreen, UnityAction)"/> or <see cref="Hide(UnityAction, LoadScreen)"/><br>
        /// Don't use LoadScreen Show/Hide methods, it must control LoadScreenManager!</br>
        /// </summary>
        /// <returns>Instance of load screen, you can freely change it timing before passing to LoadScreenManager</returns>
        public static LoadScreen CreateLoadScreen(LoadScreens type) {
            if (instance == null) {
                return null;
            }
            return instance.CreateInstance(type);
        }

        private void Awake() {
            if (instance == null) {
                instance = this;
                gameObject.SetActive(false);
            }
            else {
                Destroy(gameObject);
                return;
            }

#if UNITY_EDITOR
            if (transform.parent != null) transform.parent = null;
#endif

            DontDestroyOnLoad(gameObject);
        }

        private void Show(LoadScreen screenInstance, UnityAction callback = null) {
            if (_activeLoadScreen != null) {
                if (_activeLoadScreen.CurrentState == LoadScreenState.Showing) {
                    Debug.LogWarning($"{_activeLoadScreen.Type} is already showing, not null callback will be invoked");
                }
                else if (_activeLoadScreen.CurrentState == LoadScreenState.Hiding) {
                    _activeLoadScreen.ForceStop();
                    _activeLoadScreen.HideScreenImmediately();

                    Destroy(_activeLoadScreen.gameObject);
                    _activeLoadScreen = null;
                }
                else if (_activeLoadScreen.CurrentState == LoadScreenState.Active) {
                    callback?.Invoke();
                    return;
                }
            }

            _activeLoadScreen = screenInstance;
            if (gameObject.activeSelf == false) gameObject.SetActive(true);
            IsActive = true;

            switch (_activeLoadScreen.CurrentState) {
                case LoadScreenState.Inactive:
                case LoadScreenState.Undefined:
                    _activeLoadScreen.ShowScreen(callback);
                    return;
                case LoadScreenState.Showing:
                    if (callback != null) _activeLoadScreen.OnLoaderShown.AddListener(callback);
                    return;
                case LoadScreenState.Hiding:
                    _activeLoadScreen.ForceStop();
                    _activeLoadScreen.ShowScreen(callback);
                    break;
                case LoadScreenState.Active:
                    callback?.Invoke();
                    return;
            }
        }

        private void Show(LoadScreens type, UnityAction callback = null) {
            int index = GetIndexOf(type);
            if (index < 0) {
                Debug.LogError($"There is no {type} in array of loadScreensData!\nPicked up {_loadScreenPrefabs[0].Type} screen.");
                index = 0;
            }

            if (_activeLoadScreen != null) {
                if (_activeLoadScreen.CurrentState == LoadScreenState.Active) {
                    callback?.Invoke();
                    return;
                }
                else if (_activeLoadScreen.CurrentState == LoadScreenState.Showing) {
                    if (callback != null) _activeLoadScreen.OnLoaderShown.AddListener(callback);
                    return;
                }
            }

            var screen = CreateInstance(_loadScreenPrefabs[index]);
            Show(screen, callback);
        }

        private void Hide(UnityAction callback = null, LoadScreen hideScreenTransitionInstance = null) {
            if (_activeLoadScreen == null || _activeLoadScreen.CurrentState == LoadScreenState.Inactive) {
                callback?.Invoke();
                return;
            }

            if (hideScreenTransitionInstance != null && hideScreenTransitionInstance != _activeLoadScreen) {
                _activeLoadScreen.HideScreenImmediately();
                Destroy(_activeLoadScreen.gameObject);

                _activeLoadScreen = hideScreenTransitionInstance;
                _activeLoadScreen.ShowScreenImmediately();
            }

            UnityAction onHiden = () => {
                gameObject.SetActive(false);
                IsActive = false;
                if (_activeLoadScreen != null) Destroy(_activeLoadScreen.gameObject);
                _activeLoadScreen = null;
                callback?.Invoke();
            };

            switch (_activeLoadScreen.CurrentState) {
                case LoadScreenState.Active:
                    _activeLoadScreen.HideScreen(onHiden);
                    break;
                case LoadScreenState.Hiding:
                    if (callback != null) _activeLoadScreen.OnLoaderHiden.AddListener(callback);
                    break;
                case LoadScreenState.Inactive:
                case LoadScreenState.Undefined:
                    callback?.Invoke();
                    break;
                case LoadScreenState.Showing:
                    _activeLoadScreen.ForceStop();
                    _activeLoadScreen.HideScreen(onHiden);
                    break;
            }
        }

        private void Hide(UnityAction callback = null, LoadScreens targetHideScreenTransition = LoadScreens.None) {
            if (_activeLoadScreen == null || _activeLoadScreen.CurrentState == LoadScreenState.Inactive) {
                callback?.Invoke();
                return;
            }

            LoadScreen hideScreenTransitionInstance = null;
            if (targetHideScreenTransition != LoadScreens.None && targetHideScreenTransition != _activeLoadScreen.Type) {

                int newScreenIndex = GetIndexOf(targetHideScreenTransition);
                hideScreenTransitionInstance = CreateInstance(_loadScreenPrefabs[newScreenIndex]);
            }

            Hide(callback, hideScreenTransitionInstance);
        }

        private LoadScreen Get(LoadScreens type) {
            return _loadScreenPrefabs[GetIndexOf(type)];
        }

        private LoadScreenState GetActiveState() {
            if (_activeLoadScreen == null) return LoadScreenState.Undefined;
            return _activeLoadScreen.CurrentState;
        }

        private int GetIndexOf(LoadScreens type) {
            int index = -1;
            for (int i = 0; i < _loadScreenPrefabs.Length; i++) {
                if (_loadScreenPrefabs[i].Type == type) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private LoadScreen CreateInstance(LoadScreens type) {
            int index = GetIndexOf(type);
            if (index < 0) {
                index = 0;
                Debug.LogWarning($"Didn't find {type} load screen so took {_loadScreenPrefabs[index].Type}");
            }
            var screen = Instantiate(_loadScreenPrefabs[index], transform);
            screen.gameObject.SetActive(false);
            return screen;
        }
        private LoadScreen CreateInstance(LoadScreen prefab) {
            return Instantiate(prefab, transform);
        }
    }
}
