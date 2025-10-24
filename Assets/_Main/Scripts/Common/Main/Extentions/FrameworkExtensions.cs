using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Core;
using System.Globalization;
using UnityEngine.UI;

namespace Extensions {

    public static class ESimpleTypes {
        public static string FormatIntToString(this int number) {
            /*
                1000 = 1k
                1001 = 1k
                1050 = 1.05k
                1099 = 1.09k
                1100 = 1.1k
                9999 = 9.99k
                999999 = 999.99k
            */
            return ConvertNumberTo1k1m1b(number);
        }

        public static string FormatIntToString(this int number, string format) {
            return ConvertNumberTo1k1m1b(number, format);
        }

        public static string FormatIntToStringUpper(this int number) {
            return ConvertNumberTo1K1M1B(number);
        }

        public static string FormatIntToStringUpper(this int number, string format) {
            return ConvertNumberTo1K1M1B(number, format);
        }

        private static string ConvertNumberTo1K1M1B(int number, string format = "0.##") {
            if (number >= 1000000000) {
                return $"{(Mathf.Floor(number / 10000000f) / 100f).ToString(format, CultureInfo.InvariantCulture)}B";
            }
            else if (number >= 1000000) {
                return $"{(Mathf.Floor(number / 10000f) / 100f).ToString(format, CultureInfo.InvariantCulture)}M";
            }
            else if (number >= 1000) {
                return $"{(Mathf.Floor(number / 10f) / 100f).ToString(format, CultureInfo.InvariantCulture)}K";
            }
            else {
                return number.ToString();
            }
        }

        private static string ConvertNumberTo1k1m1b(int number, string format = "0.##") {
            if (number >= 1000000000) {
                return $"{(Mathf.Floor(number / 10000000f) / 100f).ToString(format, CultureInfo.InvariantCulture)}b";
            }
            else if (number >= 1000000) {
                return $"{(Mathf.Floor(number / 10000f) / 100f).ToString(format, CultureInfo.InvariantCulture)}m";
            }
            else if (number >= 1000) {
                return $"{(Mathf.Floor(number / 10f) / 100f).ToString(format, CultureInfo.InvariantCulture)}k";
            }
            else {
                return number.ToString();
            }
        }

        public static float ParseToFloat(this string input) {
            if (float.TryParse(input.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out float result)) {
                return result;
            }
            return 0f;
        }
    }

    public static class TimeExtentions {
        public static void UseTimeChangeInput() {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.KeypadPlus)
             || Input.GetKeyDown(KeyCode.UpArrow)) {
                float lastValue = Time.timeScale;

                if (lastValue == 0.6f) {
                    TimeManager.Instance.SetTimeScale(1f);
                }
                else {
                    lastValue += 0.5f;

                    if (lastValue > 5) {
                        TimeManager.Instance.SetTimeScale(5f);
                    }
                    else {
                        TimeManager.Instance.SetTimeScale(lastValue);
                    }
                }

                Debug.Log($"Simulation speed {Time.timeScale}");
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus)
                  || Input.GetKeyDown(KeyCode.DownArrow)) {
                float value = Time.timeScale - 0.5f;

                if (value < 0.1f) {
                    TimeManager.Instance.SetTimeScale(0.1f);
                }
                else {
                    TimeManager.Instance.SetTimeScale(value);
                }

                Debug.Log($"Simulation speed {Time.timeScale}");
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                TimeManager.Instance.SetTimeScale(1f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                TimeManager.Instance.SetTimeScale(2f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                TimeManager.Instance.SetTimeScale(3f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                TimeManager.Instance.SetTimeScale(4f);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                TimeManager.Instance.SetTimeScale(5f);
            }
#endif
        }
    }

    /// <summary>
    /// Utility extensions for querying and modifying lists.
    /// </summary>
    public static class ListExtensions {
        // Copied from https://github.com/ophilbinbriscoe/Unity-Extensions
        public static T GetRandomItem<T>(this IList<T> list) {
            if (list.Count == 0) {
                Debug.LogException(new System.InvalidOperationException("List was empty."));
                return default(T);
            }

            return list[Random.Range(0, list.Count)];
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }

        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this T[] array) {
            int n = array.Length;
            while (n > 1) {
                n--;
                int k = Random.Range(0, n + 1);
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }

        /// <summary>
        /// returns value by index or last array value if out of range
        /// </summary>
        public static T GetOrLast<T>(this T[] array, int index) {
            if (index < array.Length) return array[index];
            return array[array.Length - 1];
        }

        public static int[] ArrayOfIndices(int length) {
            if (length < 0) {
                Debug.LogException(new System.ArgumentException("Length cannot be negative."));
                return null;
            }

            int[] arr = new int[length];

            for (int i = 0; i < length; i++) {
                arr[i] = i;
            }

            return arr;
        }

        public static int[] GetArrayOfIndices<T>(this T[] array) {
            return ArrayOfIndices(array.Length);
        }

        public static int[] GetArrayOfIndices(this IList list) {
            return ArrayOfIndices(list.Count);
        }

        public static T[] Append<T>(this T[] array, T element) {
            int oldLength = array.Length;
            var copy = new T[oldLength + 1];
            for (int i = 0; i < oldLength; i++) {
                copy[i] = array[i];
            }
            copy[oldLength] = element;
            return copy;
        }
    }

    public static class EColor {
        public static Color ToGrey(this Color color) {
            float value = (color.r + color.g + color.b) * 0.3334f;
            return new Color(value, value, value, color.a);
        }
    }

    public static class RectTransformUtilities {
        public static RectSnapTypes GetSnapType(this RectTransform rectTransform) {
            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;

            if (anchorMin == Vector2.zero && anchorMax == Vector2.zero)
                return RectSnapTypes.BottomLeftCorner;
            if (anchorMin == Vector2.right && anchorMax == Vector2.right)
                return RectSnapTypes.BottomRightCorner;
            if (anchorMin == Vector2.up && anchorMax == Vector2.up)
                return RectSnapTypes.TopLeftCorner;
            if (anchorMin == Vector2.one && anchorMax == Vector2.one)
                return RectSnapTypes.TopRightCorner;
            if (anchorMin == new Vector2(1f, 0.5f) && anchorMax == new Vector2(1f, 0.5f) ||
                anchorMin == new Vector2(1f, 0f) && anchorMax == new Vector2(1f, 1f))
                return RectSnapTypes.CenterOfRightEdge;
            if (anchorMin == new Vector2(0f, 0.5f) && anchorMax == new Vector2(0f, 0.5f) ||
                anchorMin == new Vector2(0f, 0f) && anchorMax == new Vector2(0f, 1f))
                return RectSnapTypes.CenterOfLeftEdge;
            if (anchorMin == new Vector2(0.5f, 1f) && anchorMax == new Vector2(0.5f, 1f) ||
                anchorMin == new Vector2(0f, 1f) && anchorMax == new Vector2(1f, 1f))
                return RectSnapTypes.CenterOfTopEdge;
            if (anchorMin == new Vector2(0.5f, 0f) && anchorMax == new Vector2(0.5f, 0f) ||
                anchorMin == new Vector2(0f, 0f) && anchorMax == new Vector2(1f, 0f))
                return RectSnapTypes.CenterOfBottomEdge;
            if (anchorMin == new Vector2(0.5f, 0.5f) && anchorMax == new Vector2(0.5f, 0.5f)) {
                return RectSnapTypes.Center;
            }

            return RectSnapTypes.None;
        }

        public static void SetAnchoredPosY(this RectTransform rectTransform, float anchoredY) {
            var pos = rectTransform.anchoredPosition;
            pos.y = anchoredY;
            rectTransform.anchoredPosition = pos;
        }

        public static void SetAnchoredPosX(this RectTransform rectTransform, float anchoredX) {
            var pos = rectTransform.anchoredPosition;
            pos.x = anchoredX;
            rectTransform.anchoredPosition = pos;
        }
		
	public static void SetSizeX(this RectTransform rectTransform, float sizeX) {
            var size = rectTransform.sizeDelta;
            size.x = sizeX;
            rectTransform.sizeDelta = size;
        }

        public static void SetSizeY(this RectTransform rectTransform, float sizeY) {
            var size = rectTransform.sizeDelta;
            size.y = sizeY;
            rectTransform.sizeDelta = size;
        }

        public static void SetAnchorSnap(this RectTransform rectTransform, RectSnapTypes anchorSnap) {
            // init center by default
            Vector2 anchorMin = new(0.5f, 0.5f);
            Vector2 anchorMax = new(0.5f, 0.5f);

            switch (anchorSnap) {
                case RectSnapTypes.BottomLeftCorner:
                    anchorMin.x = anchorMax.y = 0f;
                    anchorMin.x = anchorMax.y = 0f;
                    break;
                case RectSnapTypes.BottomRightCorner:
                    anchorMin.x = anchorMax.x = 1f;
                    anchorMin.y = anchorMax.y = 0f;
                    break;
                case RectSnapTypes.TopLeftCorner:
                    anchorMin.x = anchorMax.x = 0f;
                    anchorMin.y = anchorMax.y = 1f;
                    break;
                case RectSnapTypes.TopRightCorner:
                    anchorMin.x = anchorMax.x = 1f;
                    anchorMin.y = anchorMax.y = 1f;
                    break;
                case RectSnapTypes.CenterOfLeftEdge:
                    anchorMin.x = anchorMax.x = 0f;
                    anchorMin.y = anchorMax.y = 0.5f;
                    break;
                case RectSnapTypes.CenterOfTopEdge:
                    anchorMin.x = anchorMax.x = 0.5f;
                    anchorMin.y = anchorMax.y = 1f;
                    break;
                case RectSnapTypes.CenterOfBottomEdge:
                    anchorMin.x = anchorMax.x = 0.5f;
                    anchorMin.y = anchorMax.y = 0f;
                    break;
                case RectSnapTypes.CenterOfRightEdge:
                    anchorMin.x = anchorMax.x = 1f;
                    anchorMin.y = anchorMax.y = 0.5f;
                    break;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to) {
            Vector2 localPoint;
            Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
            screenP += fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
            Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
            return to.anchoredPosition + localPoint - pivotDerivedOffset;
        }
    }

    public static class ETransformMethods {
        /// <summary>
        /// Checks root object name and child object names till it is equal
        /// </summary>
        /// <param name="name">child object name</param>
        /// <returns>First object with the same name by hierarchy</returns>
        public static Transform Find(this Transform parent, string name, bool includeInactive = false) {
            if (parent.gameObject.activeSelf == false && includeInactive == false) {
                return null;
            }

            if (parent.name.Equals(name)) {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++) {
                Transform foundObject = parent.GetChild(i).Find(name, includeInactive);

                if (foundObject != null) {
                    return foundObject;
                }
            }

            return null;
        }

        public static T GetComponentFirstLevelChilds<T>(this Transform parent) where T : Component {
            for (int i = 0; i < parent.childCount; i++) {
                if (parent.GetChild(i).TryGetComponent(out T component)) {
                    return component;
                }
            }

            return null;
        }


        public static void Reset(this Transform transform) {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public static void SetX(this Transform transform, float newX) {
            var position = transform.position;
            position.x = newX;
            transform.position = position;
        }

        public static void SetY(this Transform transform, float newY) {
            var position = transform.position;
            position.y = newY;
            transform.position = position;
        }

        public static void SetZ(this Transform transform, float newZ) {
            var position = transform.position;
            position.z = newZ;
            transform.position = position;
        }

        public static void SetXLocal(this Transform transform, float newX) {
            var position = transform.localPosition;
            position.x = newX;
            transform.localPosition = position;
        }

        public static void SetYLocal(this Transform transform, float newY) {
            var position = transform.localPosition;
            position.y = newY;
            transform.localPosition = position;
        }

        public static void SetZLocal(this Transform transform, float newZ) {
            var position = transform.localPosition;
            position.z = newZ;
            transform.localPosition = position;
        }
    }

    public static class EVectorMethods {
        /// <summary>
        /// returns normalized vector by angle on Ox Oy plane
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 GetVector3(this float angle) {
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static void SetSprite(this Image image, Sprite sprite, Vector2 maxSize, float scaleCoef = 1f, float zRotation = 0f) {
            Vector2 scales = maxSize / sprite.rect.size;
            image.rectTransform.sizeDelta = scaleCoef * Mathf.Min(scales.x, scales.y) * sprite.rect.size;
            image.rectTransform.rotation = Quaternion.Euler(0f, 0f, (-1f) * zRotation);
            image.sprite = sprite;
        }

        #region GitHub
        // Copied from https://gist.github.com/jringrose/5673c34a8c1c2d46d441b6050849331c
        public static float GetRadian(this Vector2 vector) {
            return Mathf.Atan2(vector.y, vector.x);
        }

        public static Vector2 GetVectorFromRadian(this float floatValue) {
            return new Vector2(Mathf.Cos(floatValue), Mathf.Sin(floatValue));
        }

        /// <summary>
        /// should be used on normalized vector
        /// </summary>
        public static float GetAngleFromDir(this Vector2 dir) {
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        public static Vector2 GetVector2(this float degrees) {
            return new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        }

        // Copied from https://github.com/ophilbinbriscoe/Unity-Extensions
        public static float GetAngleFromDir(Vector3 position, Vector3 dir) {
            var forward = position + dir;
            var angle = Mathf.Rad2Deg * Mathf.Atan2(forward.z - position.z, forward.x - position.x);
            return angle;
        }

        public static Vector3 Abs(this Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static float Max(this Vector3 v) {
            var a = (v.x > v.y ? v.x : v.y);
            return a > v.z ? a : v.z;
        }

        public static float Min(this Vector3 v) {
            var a = (v.x < v.y ? v.x : v.y);
            return a < v.z ? a : v.z;
        }
        #endregion
    }

    public static class GizmosExtensions {
        // Copied from https://github.com/code-beans/GizmoExtensions

        public static void DrawPoints(IList<Transform> points, Color color, float pointRadius = 0.25f) {
            Color oldColor = Gizmos.color;
            Gizmos.color = color;
            foreach (var point in points) {
                if (point) Gizmos.DrawSphere(point.position, pointRadius);
            }
            Gizmos.color = oldColor;
        }

        /// <summary>
        /// Draws a wire cube with a given rotation 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation = default(Quaternion)) {
            var old = Gizmos.matrix;
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = old;
        }

        public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            Gizmos.DrawLine(from, to);
            var direction = to - from;
            var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawLine(to, to + right * arrowHeadLength);
            Gizmos.DrawLine(to, to + left * arrowHeadLength);
        }

        public static void DrawWireSphere(Vector3 center, float radius, Quaternion rotation = default(Quaternion)) {
            var old = Gizmos.matrix;
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, radius);
            Gizmos.matrix = old;
        }


        /// <summary>
        /// Draws a flat wire circle (up)
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="segments"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCircle(Vector3 center, float radius, int segments = 20, Quaternion rotation = default(Quaternion)) {
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            DrawWireArc(center, radius, 360, segments, rotation);
        }

        /// <summary>
        /// Draws an arc with a rotation around the center
        /// </summary>
        /// <param name="center">center point</param>
        /// <param name="radius">radius</param>
        /// <param name="angle">angle in degrees, must be > 0.5!</param>
        /// <param name="segments">number of segments</param>
        /// <param name="rotation">rotation around the center</param>
        public static void DrawWireArc(Vector3 center, float radius, float angle, int segments = 20, Quaternion rotation = default(Quaternion)) {
            if (angle < 0.5f) {
                return;
            }

            var old = Gizmos.matrix;
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Vector3 from = Vector3.forward * radius;
            var step = Mathf.RoundToInt(angle / segments);
            if (step == 0) { step = 1; }
            for (int i = 0; i <= angle; i += step) {
                var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
                Gizmos.DrawLine(from, to);
                from = to;
            }

            Gizmos.matrix = old;
        }


        /// <summary>
        /// Draws an arc with a rotation around an arbitraty center of rotation
        /// </summary>
        /// <param name="center">the circle's center point</param>
        /// <param name="radius">radius</param>
        /// <param name="angle">angle in degrees, must be > 0.5!</param>
        /// <param name="segments">number of segments</param>
        /// <param name="rotation">rotation around the centerOfRotation</param>
        /// <param name="centerOfRotation">center of rotation</param>
        public static void DrawWireArc(Vector3 center, float radius, float angle, int segments, Quaternion rotation, Vector3 centerOfRotation) {
            if (angle < 0.5f) {
                return;
            }

            var old = Gizmos.matrix;
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            Gizmos.matrix = Matrix4x4.TRS(centerOfRotation, rotation, Vector3.one);
            var deltaTranslation = centerOfRotation - center;
            Vector3 from = deltaTranslation + Vector3.forward * radius;
            var step = Mathf.RoundToInt(angle / segments);
            if (step == 0) { step = 1; }
            for (int i = 0; i <= angle; i += step) {
                var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad)) + deltaTranslation;
                Gizmos.DrawLine(from, to);
                from = to;
            }

            Gizmos.matrix = old;
        }

        /// <summary>
        /// Draws an arc with a rotation around an arbitraty center of rotation
        /// </summary>
        /// <param name="matrix">Gizmo matrix applied before drawing</param>
        /// <param name="radius">radius</param>
        /// <param name="angle">angle in degrees, must be > 0.5!</param>
        /// <param name="segments">number of segments</param>
        private static void DrawWireArc(Matrix4x4 matrix, float radius, float angle, int segments) {
            if (angle < 0.5f) {
                return;
            }

            var old = Gizmos.matrix;
            Gizmos.matrix = matrix;
            Vector3 from = Vector3.forward * radius;
            var step = Mathf.RoundToInt(angle / segments);
            if (step == 0) { step = 1; }
            for (int i = 0; i <= angle; i += step) {
                var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
                Gizmos.DrawLine(from, to);
                from = to;
            }

            Gizmos.matrix = old;
        }

        /// <summary>
        /// Draws a wire cylinder face up with a rotation around the center
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCylinder(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion)) {
            var old = Gizmos.matrix;
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            var half = height / 2;

            //draw the 4 outer lines
            Gizmos.DrawLine(Vector3.right * radius - Vector3.up * half, Vector3.right * radius + Vector3.up * half);
            Gizmos.DrawLine(-Vector3.right * radius - Vector3.up * half, -Vector3.right * radius + Vector3.up * half);
            Gizmos.DrawLine(Vector3.forward * radius - Vector3.up * half, Vector3.forward * radius + Vector3.up * half);
            Gizmos.DrawLine(-Vector3.forward * radius - Vector3.up * half, -Vector3.forward * radius + Vector3.up * half);

            //draw the 2 cricles with the center of rotation being the center of the cylinder, not the center of the circle itself
            DrawWireArc(center + Vector3.up * half, radius, 360, 20, rotation, center);
            DrawWireArc(center + Vector3.down * half, radius, 360, 20, rotation, center);
            Gizmos.matrix = old;
        }

        /// <summary>
        /// Draws a wire capsule face up
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="rotation"></param>
        public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion)) {
            if (rotation.Equals(default(Quaternion))) {
                rotation = Quaternion.identity;
            }
            var old = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            var half = height / 2 - radius;

            //draw cylinder base
            DrawWireCylinder(center, radius, height - radius * 2);

            //draw upper cap
            //do some cool stuff with orthogonal matrices
            var mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(mat, radius, 180, 20);
            mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward));
            DrawWireArc(mat, radius, 180, 20);

            //draw lower cap
            mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(mat, radius, 180, 20);
            mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(-90, Vector3.forward));
            DrawWireArc(mat, radius, 180, 20);

            Gizmos.matrix = old;
        }
    }

    public class EnumExtention {
        public static T RandomEnumValue<T>() {
            var v = System.Enum.GetValues(typeof(T));
            return (T)v.GetValue(Random.Range(0, v.Length));
        }
    }

    public static class EInputMethods {
        public static void TopDownGame2D(out float horizontalMove, out float verticalMove) {
            if (Input.GetKey(KeyCode.D)) {
                horizontalMove = 1f;
            }
            else if (Input.GetKey(KeyCode.A)) {
                horizontalMove = -1f;
            }
            else {
                horizontalMove = 0;
            }

            if (Input.GetKey(KeyCode.W)) {
                verticalMove = 1f;
            }
            else if (Input.GetKey(KeyCode.S)) {
                verticalMove = -1f;
            }
            else {
                verticalMove = 0;
            }

            if (Mathf.Abs(verticalMove) == 1f && Mathf.Abs(horizontalMove) == 1f) {
                horizontalMove *= Mathf.Cos(45f * Mathf.Deg2Rad);
                verticalMove *= Mathf.Sin(45f * Mathf.Deg2Rad);
            }
        }

        public static void PlatformGame2D(out float horizontalMove, out bool jump, out bool crouch) {
            if (Input.GetKey(KeyCode.D)) {
                horizontalMove = 1f;
            }
            else if (Input.GetKey(KeyCode.A)) {
                horizontalMove = -1f;
            }
            else {
                horizontalMove = 0;
            }

            if (Input.GetKeyDown(KeyCode.W)) {
                jump = true;
            }
            else {
                jump = false;
            }

            if (Input.GetKey(KeyCode.S)) {
                crouch = true;
            }
            else {
                crouch = false;
            }
        }
    }
}