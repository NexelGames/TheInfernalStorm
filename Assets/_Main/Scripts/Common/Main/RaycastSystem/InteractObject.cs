using UnityEngine;
using UnityEngine.Events;
using Game.TouchRaycasters;

/// <summary>
/// Base object for InteractObjectRaycaster
/// </summary>
public abstract class InteractObject : MonoBehaviour {
    protected UnityAction _onInteractionStop;

    private bool _inProgress = false;
    /// <summary>
    /// If true then this object is being interacted by player
    /// </summary>
    public bool IsInProgress => _inProgress;

    /// <summary>
    /// Implement it to know if object locked for interactions
    /// </summary>
    public abstract bool CanInteract();

    /// <summary>
    /// This method start object interaction <br>
    /// Override it and you can implement different behaviours while interacting with object, call base() too then</br>
    /// </summary>
    /// <param name="onInteractionStop">This is callback when interaction stops</param>
    public virtual void DoInteract(UnityAction onInteractionStop = null) {
        if (onInteractionStop != null) {
            _onInteractionStop += onInteractionStop;
        }
        _inProgress = true;
    }

    /// <summary>
    /// Call this method to stop interaction <br>
    /// Override it to make custom logic, call base() too then</br>
    /// </summary>
    public virtual void StopInteract() {
        if (_onInteractionStop != null) {
            _onInteractionStop.Invoke();
            _onInteractionStop = null;
        }
        _inProgress = false;
    }

    /// <summary>
    /// Here you can subscribe on stop interact callback after interaction started
    /// </summary>
    public void AppendOnStopCallback(UnityAction onInterationStop) {
        if (onInterationStop != null) {
            _onInteractionStop += onInterationStop;
        }
    }
}