using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class InterpolatedTransformUpdater : MonoBehaviour
{
    private List<InterpolatedTransform> _interpolatedTransform = new List<InterpolatedTransform>();
    private bool _doUpdate = false;

    public void SubscribeToEvents() {
        InterpolatedTransform.OnEnabled.AddListener(AddTransform);
        InterpolatedTransform.OnDisabled.AddListener(RemoveTransform);
    }

    public void AddTransform(InterpolatedTransform interpolatedTransform) {
        _interpolatedTransform.Add(interpolatedTransform);
        if (_doUpdate == false && _interpolatedTransform.Count > 0) {
            _doUpdate = true;
        }
    }
    public void RemoveTransform(InterpolatedTransform interpolatedTransform) {
        _interpolatedTransform.Remove(interpolatedTransform);
        if (_interpolatedTransform.Count == 0) {
            _doUpdate = false;
        }
    }

    void FixedUpdate()
    {
        if (_doUpdate) {
            foreach(var transform in _interpolatedTransform) {
                transform.LateFixedUpdate();
            }
        }
    }
}
