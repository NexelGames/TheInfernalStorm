using Alchemy.Inspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

namespace Game.Core {
    public class AppStart : MonoBehaviour {
        [FoldoutGroup("Splash Screen"), SerializeField] private float _splashScreenDurationTime = 1f;
        [FoldoutGroup("Splash Screen"), SerializeField] private GameObject _logo;
        [FoldoutGroup("Splash Screen")] public bool SkipSplashScreenAnim = true;

        [FoldoutGroup("Scene to Load"), SerializeField] private string _sceneNameToLoad;

        [SerializeField] private Transform _systemsRoot;

        private IEnumerator Start() {
#if !UNITY_EDITOR
            SkipSplashScreenAnim = false;
#endif

            BeforeSplashScreenAppear();

            // uncomment to use notifications
            // yield return NotificationManager.Create();

            RunAllSystems();
            if (SkipSplashScreenAnim) {
                LoadStartScene();
                yield break;
            }

            yield return null;

            _logo.SetActive(true);

            Sequence seq = Sequence.Create();
            seq.ChainDelay(_splashScreenDurationTime);
            seq.ChainCallback(LoadStartScene);

        }

        private void LoadStartScene() {
            SceneSystem.Instance.LoadScene(_sceneNameToLoad, LoadScreens.FadeInOut);
        }

        private void BeforeSplashScreenAppear() {
            VersionValidator versionValidator = new VersionValidator(SaveSystem.Instance.Data);

            var cashManager = CashManager.Instance;
            cashManager.Init(); // awake is not called yet because object is disabled, do it manualy

            VibrationsManager.Create();

            // Refresh vSync and target FPS to avoid low target FPS.
            // note that in project shouldn't be more vSync and target FPS setup,
            // or you may overrade two lines below
            QualitySettings.vSyncCount = 0;
#if UNITY_WEBGL
            Application.targetFrameRate = -1;
#else
            Application.targetFrameRate = 120;
#endif

#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif

            SetupPrimeTweenConfig();
        }

        public static void SetupPrimeTweenConfig() {
            PrimeTweenConfig.defaultEase = Ease.InOutSine;
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        }

        private void RunAllSystems() {
            // all systems under root is disabled,
            // on enable will trigger Awake and Start methods for each system
            int childs = _systemsRoot.childCount;
            for (int i = 0; i < childs; i++) {
                _systemsRoot.GetChild(0).SetParent(null);
            }
        }
    }
}