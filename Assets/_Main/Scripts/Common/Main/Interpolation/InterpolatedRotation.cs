using UnityEngine;

[DefaultExecutionOrder(-1)]
public class InterpolatedRotation : InterpolatedTransform
{
    private Quaternion[] _lastTransforms;
    private int _newTransformIndex;

    private Transform _t;

    private void Awake() {
        _t = transform;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ForgetPreviousTransforms();
    }

    public override void ForgetPreviousTransforms()
    {
        _lastTransforms = new Quaternion[2];
        Quaternion t = _t.localRotation;
        _lastTransforms[0] = t;
        _lastTransforms[1] = t;
        _newTransformIndex = 0;
    }

    void FixedUpdate()
    {
        Quaternion newestTransform = _lastTransforms[_newTransformIndex];
        _t.localRotation = newestTransform;
    }

    public override void LateFixedUpdate()
    {
        _newTransformIndex = OldTransformIndex();
        _lastTransforms[_newTransformIndex] = _t.localRotation;
    }

    void Update()
    {
        Quaternion newestTransform = _lastTransforms[_newTransformIndex];
        Quaternion olderTransform = _lastTransforms[OldTransformIndex()];

        _t.localRotation = Quaternion.Slerp(
                                    olderTransform,
                                    newestTransform,
                                    Interpolation_Controller.InterpolationFactor);
    }

    private int OldTransformIndex()
    {
        return (_newTransformIndex == 0 ? 1 : 0);
    }
}
