using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI {
    /// <summary>
    /// Has references to all canvases <br>
    /// Is used to retrieve canvas by Canvases enum / types</br>
    /// </summary>
    public class UICanvasContainer : MonoBehaviour {
        private List<UICanvasController> _canvasInstances = new List<UICanvasController>();

        public T GetController<T>() where T : UICanvasController {
            Type targetType = typeof(T);

            foreach (UICanvasController canvas in _canvasInstances) {
                if (canvas.GetType() == targetType) {
                    return (T)canvas;
                }
            }

#if UNITY_EDITOR
            Debug.LogError($"Did not find custom canvas with Type '{typeof(T)}'.", this);
#endif
            return null;
        }

        public T GetController<T>(string objectName) where T : UICanvasController {
            foreach (UICanvasController canvas in _canvasInstances) {
                if (canvas.name.Equals(objectName) && canvas is T casted) {
                    return casted;
                }
            }

#if UNITY_EDITOR
            Debug.LogError($"Did not find custom canvas with Type '{typeof(T)}' and name {objectName}.", this);
#endif
            return null;
        }

        public void DisableAll() {
            if (_canvasInstances.Count > 0) {
                foreach (var canvas in _canvasInstances) {
                    if (canvas.IsActive) {
                        canvas.EnableCanvas(false);
                    }
                }
            }
        }

        internal void OnSceneChanged() {
            // clear up UI (to avoid UI state live)
            if (_canvasInstances.Count > 0) {
                foreach (var canvas in _canvasInstances) {
                    if (canvas != null) {
                        canvas.StopTransition();
                        Destroy(canvas.gameObject);
                    }
                }
                _canvasInstances.Clear();
            }
        }

        internal void OnCanvasCreated(UICanvasController newCanvas) {
            if (newCanvas.IsChildCanvas == false) {
                newCanvas.transform.SetParent(UIManager.instance.transform, worldPositionStays: false);
            }
            _canvasInstances.Add(newCanvas);
        }
    }
}
