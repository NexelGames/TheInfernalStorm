using UnityEngine;

namespace Game.Core {
    public class PoolArrayBuffer<T> : ArrayBuffer<T> where T : PoolManager.ObjectInstance {
        public PoolArrayBuffer(T[] objects, PoolManager.PoolSettings settings, Transform holder) : base(objects) {
            PrefabIn = settings.Prefab;
            AutoFillBuffer = settings.AutoFillPool;
            AutoFillCount = settings.AutoFillCount;
            HolderTransform = holder;
            Size = objects.Length;
        }

        public int Size;
        public Transform HolderTransform;
        public bool AutoFillBuffer = true;
        public int AutoFillCount;
        public GameObject PrefabIn;

        /// <summary>
        /// Checks if current cursor object is busy without cursor move
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentObjectBusy() {
            return _array[_curIndex].IsObjectBusy;
        }

        /// <summary>
        /// Reuses current cursor ObjectInstance without cursor move
        /// </summary>
        public void ReuseCurrentObjectInstance() {
            _array[_curIndex].Reuse();
        }

        /// <summary>
        /// Reuses current cursor ObjectInstance without cursor move
        /// </summary>
        public void ReuseCurrentObjectInstance(Vector3 atPosition, Quaternion withRotation) {
            _array[_curIndex].Reuse(atPosition, withRotation);
        }

        /// <summary>
        /// Returns pool object of current cursor object without cursor move
        /// </summary>
        public PoolManager.ObjectInstance PeakCurrentObjectInstance() {
            return _array[_curIndex];
        }

        /// <summary>
        /// Destroys all items in array (MonoBehaviour.Destroy)
        /// </summary>
        public void CleanUp() {
            foreach (var item in _array) {
                item.DestroyCompletely();
            }
            MonoBehaviour.Destroy(HolderTransform.gameObject);
        }
    }
}