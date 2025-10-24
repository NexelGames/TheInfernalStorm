using UnityEngine;

namespace Game.UI {
    public class SceneUIContainer : MonoBehaviour {
        [SerializeField] private UICanvasController _canvasToActivate;

        public void Setup() {
            UICanvasController[] canvases = GetComponentsInChildren<UICanvasController>(includeInactive: true);

            foreach (var canvas in canvases) {
                if (canvas.gameObject.activeSelf) {
                    canvas.gameObject.SetActive(false);
                }
            }

            foreach (var canvas in canvases) {
                canvas.InitReferences();
                // raise even if object inactive
                canvas.RaiseOnCreateEvent();
            }

            if (_canvasToActivate) {
                UIManager.instance.SwitchCanvasTo(_canvasToActivate);
            }


            if (transform.childCount == 0) {
                Destroy(gameObject);
            }
        }
    }
}