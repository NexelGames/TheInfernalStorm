using UnityEngine;

public static class DebugExtensions {
    public static void DrawCube(Vector3 position, Vector3 size, float duration = 0.1f) {
        DrawCube(position, size, Quaternion.identity, Color.red, duration);
    }

    public static void DrawCube(Vector3 position, Vector3 size, Quaternion rotation, float duration = 0.1f) {
        DrawCube(position, size, rotation, Color.red, duration);
    }

    public static void DrawCube(Vector3 position, Vector3 size, Quaternion rotation, Color color, float duration = 0.1f) {
        Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, size);

        // Draw the cube using debug lines
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(-0.5f, -0.5f, -0.5f);
        corners[1] = new Vector3(-0.5f, -0.5f, 0.5f);
        corners[2] = new Vector3(-0.5f, 0.5f, -0.5f);
        corners[3] = new Vector3(-0.5f, 0.5f, 0.5f);
        corners[4] = new Vector3(0.5f, -0.5f, -0.5f);
        corners[5] = new Vector3(0.5f, -0.5f, 0.5f);
        corners[6] = new Vector3(0.5f, 0.5f, -0.5f);
        corners[7] = new Vector3(0.5f, 0.5f, 0.5f);

        for (int i = 0; i < corners.Length; i++) {
            corners[i] = matrix.MultiplyPoint(corners[i]);
        }

        Debug.DrawLine(corners[0], corners[1], color, duration);
        Debug.DrawLine(corners[0], corners[2], color, duration);
        Debug.DrawLine(corners[0], corners[4], color, duration);
        Debug.DrawLine(corners[1], corners[3], color, duration);
        Debug.DrawLine(corners[1], corners[5], color, duration);
        Debug.DrawLine(corners[2], corners[3], color, duration);
        Debug.DrawLine(corners[2], corners[6], color, duration);
        Debug.DrawLine(corners[3], corners[7], color, duration);
        Debug.DrawLine(corners[4], corners[5], color, duration);
        Debug.DrawLine(corners[4], corners[6], color, duration);
        Debug.DrawLine(corners[5], corners[7], color, duration);
        Debug.DrawLine(corners[6], corners[7], color, duration);
    }

    public static void DrawCircle(Vector3 center, float radius, float duration = 0.1f) {
        DrawCircle(center, radius, Quaternion.identity, Color.red, 24, duration);
    }

    public static void DrawCircle(Vector3 center, float radius, Quaternion rotation, float duration = 0.1f) {
        DrawCircle(center, radius, rotation, Color.red, 24, duration);
    }

    public static void DrawCircle(Vector3 center, float radius, Quaternion rotation, Color color, int numSegments, float duration = 0.1f) {
        Vector3 prevPoint = center + rotation * new Vector3(radius, 0, 0);

        for (int i = 1; i <= numSegments; i++) {
            float angle = i / (float)numSegments * 360f;
            Vector3 currPoint = center + rotation * Quaternion.Euler(0, 0, angle) * new Vector3(radius, 0, 0);
            Debug.DrawLine(prevPoint, currPoint, color, duration);
            prevPoint = currPoint;
        }

        Debug.DrawLine(prevPoint, center + rotation * new Vector3(radius, 0, 0), color, duration);
    }
}