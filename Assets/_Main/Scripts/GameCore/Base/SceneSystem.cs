using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Game.Core {
    /// <summary>
    /// Main class to handle Scene switch/load operations
    /// </summary>
    public class SceneSystem {
        #region Singleton
        private static SceneSystem _instance;
        public static SceneSystem Instance {
            get {
                if (_instance == null) _instance = new SceneSystem();
                return _instance;
            }
        }
        #endregion

        public static Scene SingleActiveScene;
        public static Scene SinglePreviousScene;

        public bool VerboseLogs;

        /// <summary>
        /// UnityAction, invokes when scene is full ready to be used <br>
        /// You can use it to enable AI or activate play timer and etc.</br> <br>
        /// If loadScreen was - event executed on start of loadScreen disappear. </br> <br>
        /// In all, this event invokes only when <see cref="Initialization"/> completed (if no initialization running - instanly as soon as scene loads)</br>
        /// </summary>
        [HideInInspector]
        public static UnityEvent OnSceneFullReady = new();
        public static UnityEvent OnSingleModeSceneStartLoading = new();

        private Coroutine _sceneLoading;
        private Coroutine _scriptsInitialization;
        private List<AsyncOperation> _scenesLoad = new();

        private LoadScreen _hideLoadScreenTransition;

        public SceneSystem() {
            SceneManager.sceneLoaded += OnManagerSceneLoaded;

            SingleActiveScene = SceneManager.GetActiveScene();

            if (SingleActiveScene.name != "_Loader") {
                CoroutineHelper.WaitForAction(InvokeSceneFullReadyEvent, 0.5f);
            }
        }

        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadScene(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single, bool asyncLoad = false) {
            StartSceneLoad(sceneName, asyncLoad, loadMode);
        }

        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadScene(string sceneName, LoadScreens loadScreen, LoadSceneMode loadMode = LoadSceneMode.Single, bool asyncLoad = false) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreen, () => { StartSceneLoad(sceneName, asyncLoad, loadMode); });
            _hideLoadScreenTransition = screen;
        }

        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadScene(string sceneName, LoadScreens loadScreenIn, LoadScreens loadScreenOut, LoadSceneMode loadMode = LoadSceneMode.Single, bool asyncLoad = false) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, asyncLoad, loadMode); });
            _hideLoadScreenTransition = LoadScreenManager.CreateLoadScreen(loadScreenOut);
        }

        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadScene(string sceneName, LoadScreens loadScreenIn, LoadScreen loadScreenOut, LoadSceneMode loadMode = LoadSceneMode.Single, bool asyncLoad = false) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, asyncLoad, loadMode); });
            _hideLoadScreenTransition = loadScreenOut;
        }

        /// <param name="sceneName">Scene name that exists in build settings</param>
        /// <param name="sceneType">Define type of load scene, if it's level then <see cref="SceneTypes.LevelScene"/> and etc</param>
        public void LoadScene(string sceneName, LoadScreen loadScreenIn, LoadScreen loadScreenOut = null, LoadSceneMode loadMode = LoadSceneMode.Single, bool asyncLoad = false) {
            LoadScreenManager.ShowLoadScreen(loadScreenIn, () => { StartSceneLoad(sceneName, asyncLoad, loadMode); });
            _hideLoadScreenTransition = loadScreenOut ? loadScreenOut : loadScreenIn;
        }

        public void AdditiveScenesChange(string[] unloadScenes, string[] loadScenes, bool asyncLoad = false) {
            foreach (var scene in unloadScenes) {
                SceneManager.UnloadSceneAsync(scene);
            }
            StartScenesLoadAdditive(loadScenes, asyncLoad);
        }

        public void AdditiveScenesChange(string[] unloadScenes, string[] loadScenes, Action beforeSceneUnload, 
                                       LoadScreens loadScreen, bool asyncLoad = false) {
            AdditiveScenesChange(unloadScenes, loadScenes, beforeSceneUnload, loadScreen, loadScreen, asyncLoad);
        }

        public void AdditiveScenesChange(string[] unloadScenes, string[] loadScenes, Action beforeSceneUnload, 
                                       LoadScreens loadScreenIn, LoadScreens loadScreenOut, bool asyncLoad = false) {
            var screen = LoadScreenManager.ShowLoadScreen(loadScreenIn, () => {
                beforeSceneUnload?.Invoke();
                foreach (var scene in unloadScenes) {
                    SceneManager.UnloadSceneAsync(scene);
                }
                StartScenesLoadAdditive(loadScenes, asyncLoad);
            });

            if (loadScreenOut != loadScreenIn) {
                _hideLoadScreenTransition = LoadScreenManager.CreateLoadScreen(loadScreenOut);
            }
            else {
                _hideLoadScreenTransition = screen;
            }
        }

        public void AdditiveScenesChange(string[] unloadScenes, string[] loadScenes, Action beforeSceneUnload, 
                                       LoadScreens loadScreenIn, LoadScreen loadScreenOut, bool asyncLoad = false) {
            LoadScreenManager.ShowLoadScreen(loadScreenIn, () => {
                beforeSceneUnload?.Invoke();
                foreach (var scene in unloadScenes) {
                    SceneManager.UnloadSceneAsync(scene);
                }
                StartScenesLoadAdditive(loadScenes, asyncLoad);
            });

            _hideLoadScreenTransition = loadScreenOut;
        }

        public void AdditiveScenesChange(string[] unloadScenes, string[] loadScenes, Action beforeSceneUnload, 
                                       LoadScreen loadScreenIn, LoadScreen loadScreenOut = null, bool asyncLoad = false) {
            LoadScreenManager.ShowLoadScreen(loadScreenIn, () => {
                beforeSceneUnload?.Invoke();
                foreach (var scene in unloadScenes) {
                    SceneManager.UnloadSceneAsync(scene);
                }
                StartScenesLoadAdditive(loadScenes, asyncLoad);
            });

            _hideLoadScreenTransition = loadScreenOut ? loadScreenOut : loadScreenIn;
        }

        /// <summary>
        /// Iterates through scenes in hierarchy and sets as active if found
        /// </summary>
        /// <param name="withName">target scene to be active</param>
        public void SetActiveScene(string withName) {
            int count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(withName)) {
                    SceneManager.SetActiveScene(scene);
                    SingleActiveScene = scene;
                }
            }
        }

        /// <summary>
        /// Reload active scene immediately
        /// </summary>
        public void ReloadActiveScene(bool async = true) {
            LoadScene(SingleActiveScene.name, LoadSceneMode.Single, asyncLoad: async);
        }

        /// <summary>
        /// Reload active scene with LoadScreen
        /// </summary>
        public void ReloadActiveScene(LoadScreens loadScreenType, bool async = true) {
            LoadScene(SingleActiveScene.name, loadScreenType, LoadSceneMode.Single, async);
        }

        /// <summary>
        /// when scene is ready to be used
        /// </summary>
        private void InvokeSceneFullReadyEvent() {
            OnSceneFullReady.Invoke();
        }

        private void OnManagerSceneLoaded(Scene loadedScene, LoadSceneMode loadMode) {
            if (loadMode != LoadSceneMode.Single) {
                return;
            }

            SinglePreviousScene = SingleActiveScene;
            SingleActiveScene = loadedScene;
        }

        private void StartSceneLoad(string sceneName, bool async, LoadSceneMode loadMode = LoadSceneMode.Single) {
            if (_sceneLoading != null || _scriptsInitialization != null) {
                if (VerboseLogs) {
                    Debug.LogError("Scene is already loading or scripts is not initialized yet!");
                }
                return;
            }

            if (VerboseLogs) Debug.Log($"Start loading scene {sceneName}!");
            if (loadMode == LoadSceneMode.Single) OnSingleModeSceneStartLoading.Invoke();

            if (async) _scenesLoad.Add(SceneManager.LoadSceneAsync(sceneName, loadMode));
            else SceneManager.LoadScene(sceneName, loadMode);

            _sceneLoading = CoroutineHelper.Run(SceneLoadProgress(loadMode));
        }

        private void StartScenesLoadAdditive(string[] scenes, bool async) {
            if (_sceneLoading != null || _scriptsInitialization != null) {
                if (VerboseLogs) {
                    Debug.LogError("Scene is already loading or scripts is not initialized yet!");
                }
                return;
            }

            if (VerboseLogs) Debug.Log($"Start loading scenes additive: {scenes[0]} θ εωΈ {scenes.Length - 1} ρφεν!");

            if (async) {
                foreach (var scene in scenes) {
                    _scenesLoad.Add(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive));
                }
            }
            else {
                foreach (var scene in scenes) {
                    SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
                }
            }

            _sceneLoading = CoroutineHelper.Run(SceneLoadProgress(LoadSceneMode.Additive));
        }

        /// <summary>
        /// progress [0..1]
        /// </summary>
        float _totalSceneLoadProgress;
        float _totalInitialisationProgress;
        private IEnumerator SceneLoadProgress(LoadSceneMode loadMode) {
            for (int i = 0; i < _scenesLoad.Count; i++) {
                while (!_scenesLoad[i].isDone) {
                    _totalSceneLoadProgress = 0f;

                    foreach (AsyncOperation operation in _scenesLoad) {
                        _totalSceneLoadProgress += operation.progress;
                    }

                    _totalSceneLoadProgress /= _scenesLoad.Count;
                    if (VerboseLogs) Debug.Log($"Scene load progress: {_totalSceneLoadProgress}!");
                    yield return null;
                }
            }

            _totalSceneLoadProgress = 1f;

            // as soon as scene is loaded - start loading scripts
            _scriptsInitialization = CoroutineHelper.Run(GetTotalProgress());

            _scenesLoad.Clear();
            _sceneLoading = null;
        }

        private IEnumerator GetTotalProgress() {
            // wait 1 frame to ensure that Initialization object is created
            yield return null;

            float totalProgress;
            LoadScreen loadScreen = null;
            if (LoadScreenManager.IsActive) {
                loadScreen = LoadScreenManager.GetActiveScreen();
            }

            if (Initialization.current) {
                while (!Initialization.current.IsDone) {
                    _totalInitialisationProgress = Initialization.current.GetInitProgress();

                    // [0..1]
                    totalProgress = (_totalInitialisationProgress + _totalSceneLoadProgress) / 2f;
                    if (loadScreen != null) loadScreen.OnUpdateLoadProgress(totalProgress);
                    yield return null;
                }
            }

            totalProgress = 1f;
            if (loadScreen != null) {
                loadScreen.OnUpdateLoadProgress(totalProgress);
                LoadScreenManager.HideLoadScreen(_hideLoadScreenTransition);
            }
            else if (_hideLoadScreenTransition != null) {
                GameObject.Destroy(_hideLoadScreenTransition);
                _hideLoadScreenTransition = null;
            }

            _scriptsInitialization = null;
            InvokeSceneFullReadyEvent();
        }
    }
}