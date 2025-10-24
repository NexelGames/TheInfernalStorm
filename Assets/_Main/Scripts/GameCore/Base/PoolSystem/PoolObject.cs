using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Game.Core {

    /// <summary>
    /// Base class for pool objects.
    /// Inherit it and use PoolObject type for serialize field
    /// </summary>
    public class PoolObject : MonoBehaviour {
        public static UnityEvent OnDeactivateAllPoolObjects = new UnityEvent();

        private bool _isPoolObjectUsed = false;
        /// <summary>
        /// Is in use = gameObject.activeSelf <br>
        /// Used in pool system to skip object if it's in use</br>
        /// </summary>
        public bool IsPoolObjectUsed => _isPoolObjectUsed;

        public virtual void OnObjectReuse() {
            OnDeactivateAllPoolObjects.AddListener(Destroy);
            _isPoolObjectUsed = true;
        }

        /// <summary>
        /// Used to turn off object and put back into pool
        /// </summary>
        public void Destroy() {
            OnDeactivateAllPoolObjects.RemoveListener(Destroy);
            if (_destroyDelay != null) {
                StopCoroutine(_destroyDelay);
            }
            gameObject.SetActive(false);
            _isPoolObjectUsed = false;
        }

        /// <summary>
        /// Used to turn off object and put back into pool
        /// </summary>
        /// <param name="delay">delay before turn off and back to pool</param>
        public void Destroy(float delay) {
            if (_destroyDelay == null) {
                _destroyDelay = StartCoroutine(DestroyDelay(delay));
            }
        }

        private Coroutine _destroyDelay = null;
        IEnumerator DestroyDelay(float delay) {
            yield return new WaitForSeconds(delay);
            _destroyDelay = null;
            Destroy();
        }
    }
}