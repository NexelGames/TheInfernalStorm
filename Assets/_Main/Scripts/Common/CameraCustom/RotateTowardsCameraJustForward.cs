using UnityEngine;

[AddComponentMenu("Common/Camera/Rotate Towards Camera Only Forward")]
public class RotateTowardsCameraJustForward : MonoBehaviour
{
    private Transform _t;
    private Transform _cameraTransform;

    [SerializeField]
    private bool _inverseForward = true;
    private float _forwardCoef = -1f;

    private void Awake() {
        _t = transform;
        if (_inverseForward) {
            _forwardCoef *= -1;
        }
    }

    private void Start() {
        _cameraTransform = Camera.main.transform;
    }

    private void Update() {
        _t.rotation = Quaternion.LookRotation(_forwardCoef * _cameraTransform.forward, Vector3.up);
    }
}
