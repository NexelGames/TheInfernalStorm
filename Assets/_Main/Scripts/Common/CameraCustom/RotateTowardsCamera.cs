using UnityEngine;

[AddComponentMenu("Common/Camera/Rotate Towards Camera")]
public class RotateTowardsCamera : MonoBehaviour
{
    private Transform _t;
    private Transform _cameraTransform;

    private void Awake() {
        _t = transform;
    }

    private void Start() {
        _cameraTransform = Camera.main.transform;
    }

    private void Update() {
        _t.rotation = Quaternion.LookRotation((_cameraTransform.position - _t.position).normalized, Vector3.up);
    }
}
