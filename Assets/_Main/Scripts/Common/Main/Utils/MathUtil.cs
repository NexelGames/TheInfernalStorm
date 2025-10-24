using UnityEngine;


/// <summary>
/// Custom functions math related
/// </summary>
public static class MathUtil {
    /// <summary>
    /// Interpolate between three point by t [0..1]
    /// ref: https://javascript.info/bezier-curve
    /// </summary>
    public static Vector3 Lerp3(Vector3 start, Vector3 middle, Vector3 end, float t) {
        return (1f - t) * (1f - t) * start + 2f * (1f - t) * t * middle + t * t * end;
    }
}