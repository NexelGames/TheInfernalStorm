using UnityEngine;

[DefaultExecutionOrder(-1)]
public class InterpolateAll : InterpolatedTransform 
{
    private TransformData[] _lastTransforms;
    private int _newTransformIndex;

    protected override void OnEnable() {
        base.OnEnable();
        ForgetPreviousTransforms();
    }

    public override void ForgetPreviousTransforms() {
        _lastTransforms = new TransformData[2];
        TransformData t = new TransformData(
                                transform.localPosition,
                                transform.localRotation,
                                transform.localScale);
        _lastTransforms[0] = t;
        _lastTransforms[1] = t;
        _newTransformIndex = 0;
    }

    void FixedUpdate() {
        TransformData newestTransform = _lastTransforms[_newTransformIndex];
        transform.localPosition = newestTransform.Position;
        transform.localRotation = newestTransform.Rotation;
        transform.localScale = newestTransform.Scale;
    }

    public override void LateFixedUpdate() {
        _newTransformIndex = OldTransformIndex();
        _lastTransforms[_newTransformIndex] = new TransformData(
                                                    transform.localPosition,
                                                    transform.localRotation,
                                                    transform.localScale);
    }

    void Update() {
        TransformData newestTransform = _lastTransforms[_newTransformIndex];
        TransformData olderTransform = _lastTransforms[OldTransformIndex()];

        transform.localPosition = Vector3.Lerp(
                                    olderTransform.Position,
                                    newestTransform.Position,
                                    Interpolation_Controller.InterpolationFactor);
        transform.localRotation = Quaternion.Slerp(
                                    olderTransform.Rotation,
                                    newestTransform.Rotation,
                                    Interpolation_Controller.InterpolationFactor);
        transform.localScale = Vector3.Lerp(
                                    olderTransform.Scale,
                                    newestTransform.Scale,
                                    Interpolation_Controller.InterpolationFactor);
    }

    private int OldTransformIndex() {
        return (_newTransformIndex == 0 ? 1 : 0);
    }

    private struct TransformData {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale) {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }
    }
}
