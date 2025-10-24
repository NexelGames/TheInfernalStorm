using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1)]
public abstract class InterpolatedTransform : MonoBehaviour
{
    public static UnityEvent<InterpolatedTransform> OnEnabled = new UnityEvent<InterpolatedTransform>();
    public static UnityEvent<InterpolatedTransform> OnDisabled = new UnityEvent<InterpolatedTransform>();

    protected virtual void OnEnable() {
        OnEnabled.Invoke(this);
    }
    protected virtual void OnDisable() {
        OnDisabled.Invoke(this);
    }

    /// <summary>
    /// Use this method when teleporting object (change pos without interpolation) <br>
    /// Call it after position change in the end</br>
    /// </summary>
    public abstract void ForgetPreviousTransforms();
    public abstract void LateFixedUpdate();
}
