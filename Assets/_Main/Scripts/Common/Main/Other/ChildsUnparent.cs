using UnityEngine;

namespace Game {
    public class ChildsUnparent : MonoBehaviour {
        [SerializeField] private bool _destroyOnComplete = true;

        private void Awake() {
            int childs = transform.childCount;
            for (int i = 0; i < childs; i++) {
                transform.GetChild(0).parent = null;
            }

            if (_destroyOnComplete) {
                Destroy(gameObject);
            }
        }
    }
}