using System;
using UnityEngine;

namespace Game.TouchRaycasters {
    public class RaycastManager : MonoBehaviour {
        public static RaycastManager instance;

        private TouchRaycaster[] _allRaycasters;

        private TouchRaycaster _activeRaycaster;
        /// <summary>
        /// Raycaster that is currently active
        /// </summary>
        public TouchRaycaster ActiveRaycaster => _activeRaycaster;

        private void Awake() {
            if (instance == null) {
                instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _allRaycasters = GetComponents<TouchRaycaster>();
        }

        /// <summary>
        /// Enables only one raycaster by type
        /// </summary>
        /// <typeparam name="T">Raycaster type (class)</typeparam>
        public void EnableOnly<T>() where T : TouchRaycaster {
            var newRaycaster = GetRaycaster<T>();
            if (newRaycaster) {
                EnableOnly(newRaycaster);
            }
            else {
                Debug.LogError($"No such {typeof(T)} raycaster!");
            }
        }

        /// <summary>
        /// Enables only one raycaster
        /// </summary>
        /// <typeparam name="newRaycaster">Raycaster object</typeparam>
        public void EnableOnly(TouchRaycaster newRaycaster) {
            foreach (var raycaster in _allRaycasters) {
                if (raycaster.UseRaycaster) {
                    raycaster.UseRaycaster = false;
                }
            }
            if (newRaycaster) {
                newRaycaster.UseRaycaster = true;
            }
            _activeRaycaster = newRaycaster;
        }

        /// <summary>
        /// Disables active raycaster if it's not null
        /// </summary>
        public void DisableActiveRaycaster() {
            if (_activeRaycaster != null) {
                _activeRaycaster.UseRaycaster = false;
                _activeRaycaster = null;
            }
        }

        /// <summary>
        /// Disables raycaster object if it's active
        /// </summary>
        /// <param name="raycaster"></param>
        public void Disable(TouchRaycaster raycaster) {
            if (raycaster == _activeRaycaster && _activeRaycaster != null) {
                _activeRaycaster.UseRaycaster = false;
                _activeRaycaster = null;
            }
            else if (raycaster && raycaster.UseRaycaster) {
                raycaster.UseRaycaster = false;
            }
        }

        private T GetRaycaster<T>() where T : TouchRaycaster {
            Type targetType = typeof(T);
            foreach (var raycaster in _allRaycasters) {
                if (raycaster.GetType() == targetType) {
                    return (T)raycaster;
                }
            }
            return null;
        }
    }
}