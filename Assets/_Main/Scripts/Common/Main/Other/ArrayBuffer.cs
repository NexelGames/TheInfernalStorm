
using System.Collections;
using System.Collections.Generic;

namespace Game.Core {
    public class ArrayBuffer<T> : IEnumerable<T> {
        public int Length => _arrayLength;

        protected int _curIndex = 0;
        protected T[] _array;
        protected int _arrayLength;

        public ArrayBuffer(T[] objects) {
            _array = objects;
            _arrayLength = objects.Length;
        }

        public T this[int index] {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <summary>
        /// Returns current cursor position element<br>
        /// And increments cursor position</br>
        /// </summary>
        /// <returns></returns>
        public T GetNext() {
            return _array[GetCurrentIndexAndIncrement()];
        }

        /// <summary>
        /// Increments current cursor position, skips element
        /// </summary>
        public void SkipCurrent() {
            _curIndex++;
            if (_curIndex == _arrayLength) {
                _curIndex = 0;
            }
        }

        /// <summary>
        /// Returns current cursor position element<br>
        /// without cursor position icrement</br>
        /// </summary>
        /// <returns></returns>
        public T Peak() {
            return _array[_curIndex];
        }

        /// <summary>
        /// Increases buffer by adding objects array
        /// </summary>
        public void Add(T[] objects) {
            int newArrayLength = _arrayLength + objects.Length;
            T[] newArray = new T[newArrayLength];

            // copy old
            for (int i = 0; i < _arrayLength; i++) {
                newArray[i] = _array[i];
            }

            // add new
            for (int i = _arrayLength; i < newArrayLength; i++) {
                newArray[i] = objects[i - _arrayLength];
            }

            _arrayLength = newArrayLength;
            _array = newArray;
        }

        /// <summary>
        /// Increases buffer by adding 1 object
        /// </summary>
        public void Add(T newObject) {
            int newArrayLength = _arrayLength + 1;
            T[] newArray = new T[newArrayLength];

            // copy old
            for (int i = 0; i < _arrayLength; i++) {
                newArray[i] = _array[i];
            }

            // add new
            newArray[newArrayLength - 1] = newObject;

            _arrayLength = newArrayLength;
            _array = newArray;
        }

        #region IEnumerable<T> implementation
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _arrayLength; i++) {
                yield return _array[i];
            }
        }
        #endregion
        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        private int GetCurrentIndexAndIncrement() {
            _curIndex++;
            if (_curIndex < _arrayLength) {
                return _curIndex - 1;
            }
            _curIndex = 0;
            return _arrayLength - 1;
        }
    }
}