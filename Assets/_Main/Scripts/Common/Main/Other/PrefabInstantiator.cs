using UnityEngine;

/// <summary>
/// Base class for auto prefab instantiators
/// </summary>
public abstract class PrefabInstantiator : MonoBehaviour
{
    public GameObject Prefab;
    public bool DestroyEmptyObj = true;

    public void Awake() {
        if (CanCreate()) {
            OnPrefabInstantiated(Instantiate(Prefab));
        }
        
        if (DestroyEmptyObj) {
            Destroy(gameObject);
        }
    }

    protected abstract bool CanCreate();

    /// <summary>
    /// Override to do smth with createdObj
    /// </summary>
    protected abstract void OnPrefabInstantiated(GameObject createdObj);
}
