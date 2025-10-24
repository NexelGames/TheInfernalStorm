using UnityEngine;
using UnityEngine.Events;

namespace Game.Core {

    [System.Serializable]
    public enum LoadScreenState : byte {
        Undefined,
        Inactive,
        Showing,
        Hiding,
        Active,
    }

    /// <summary>
    /// Base class for all load screens objects
    /// </summary>
    public abstract class LoadScreen : MonoBehaviour {

        /// <summary>
        /// it needs to be cleared by RemoveAllListeners func in child class!
        /// </summary>
        [HideInInspector] public UnityEvent OnLoaderShown;

        /// <summary>
        /// it needs to be cleared by RemoveAllListeners func in child class!
        /// </summary>
        [HideInInspector] public UnityEvent OnLoaderHiden;

        public LoadScreens Type => _type;
        public abstract float TransitionInTime { get; set; }
        public abstract float TransitionOutTime { get; set; }
        public T CastTo<T>() where T : LoadScreen {
            return (T)this;
        }

        [SerializeField] private LoadScreens _type;

        /// <summary>
        /// Current state of load screen <br>
        /// If you implementing your custom - you should change it in LoadScreen sub class</br>
        /// </summary>
        public abstract LoadScreenState CurrentState { get; protected set; }

        /// <summary>
        /// Override it if you want to fill bar / set text of load progress by other scripts <br>
        /// Implement how you will apply loadingProgressValue yourself for specific LoadScreen type</br>
        /// </summary>
        public abstract void OnUpdateLoadProgress(float loadingProgressValue);

        /// <summary>
        /// Call it to show loader screen <br>
        /// It will invoke OnLoaderShown event</br>
        /// </summary>
        internal abstract void ShowScreen(UnityAction callback = null);

        internal abstract void ShowScreenImmediately();

        /// <summary>
        /// Call it to hide loader screen <br>
        /// It will invoke OnLoaderHiden event</br>
        /// </summary>
        internal abstract void HideScreen(UnityAction callback = null);

        internal abstract void HideScreenImmediately();

        /// <summary>
        /// Stops show/hide process
        /// </summary>
        internal abstract void ForceStop();

    }
}