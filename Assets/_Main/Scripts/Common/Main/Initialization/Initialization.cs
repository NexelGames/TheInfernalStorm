using System.Collections.Generic;
using UnityEngine;

namespace Game.Core {

    /// <summary>
    /// Initialization script<br>
    /// Use it when you need to initialize script while scene loading or smth else</br>
    /// </summary>
    public class Initialization : MonoBehaviour {
        public static Initialization current;
        public static Initialization CreateOrGetCurrent() {
            if (current == null) {
                GameObject initObj = new GameObject("InitializationObject");
                current = initObj.AddComponent<Initialization>();
            }
            return current;
        }

        /// <summary>
        /// Is initialization process is done
        /// </summary>
        public bool IsDone { get; private set; } = false;

        private void Awake() {
            if (current == null) {
                current = this;
            }
            else {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Container for all scripts with initialization state
        /// </summary>
        private List<IInitializable> _initProcesses = new List<IInitializable>();

        /// <summary>
        /// Adds script initialization proc<br>
        /// Must be called at the start of script initialization</br>
        /// </summary>
        public void AddInitProc(IInitializable script) {
            if (_initProcesses.Contains(script)) {
                Debug.LogError("script is already added to initialization current!");
                return;
            }

            IsDone = false;
            _initProcesses.Add(script);
        }

        /// <summary>
        /// Removes script initialization proc<br>
        /// Must be called at the end of script initialization</br>
        /// </summary>
        public void RemoveInitProc(IInitializable script) {
            if (_initProcesses.Contains(script)) {
                _initProcesses.Remove(script);
                if (_initProcesses.Count == 0) {
                    IsDone = true;
                }
            }
            else {
                Debug.LogError("script isn't added to initialization current!");
                return;
            }
        }

        /// <summary>
        /// Returns total progress of initialization<br>
        /// It uses information from all scripts that was added by AddInitProc func</br><br>
        /// Script that is using IInitializable interface must return progress from 0f to 1f!</br>
        /// </summary>
        /// <returns>total progress [0..1], if all scripts returns progress in range [0..1]</returns>
        public float GetInitProgress() {
            if (_initProcesses.Count == 0) { return IsDone ? 1f : 0f; }

            float progress = 0f;
            foreach (IInitializable process in _initProcesses) {
                progress += process.GetInitProgress();
            }
#if UNITY_EDITOR
            // print("init progress: " + (progress / initProcesses.Count));
#endif
            return progress / _initProcesses.Count;
        }
    }
}