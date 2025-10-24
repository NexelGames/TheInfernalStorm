using UnityEngine;
using UnityEditor;

namespace Game.EditorTools {
    public class PrefabRemoveCollider : MonoBehaviour {
        [MenuItem("Tools/Remove Colliders Selected Objs")]
        public static void RemoveColliderAndApplyPrefabChanges() {
            string log = "";
            var obj = Selection.gameObjects;
            if (obj != null && obj.Length != 0) {
                for (int i = 0; i < obj.Length; i++) {
#pragma warning disable CS0618 // Type or member is obsolete
                    var prefab_root = PrefabUtility.FindPrefabRoot(obj[i]);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (prefab_root != null) {
                        log += "<color=white>CHECKING PREFAB:</color> " + obj[i].name + "\n";

                        // now check to see if has a collider
                        Collider[] colliders = obj[i].GetComponentsInChildren<Collider>();
                        foreach (Collider collider in colliders) {
                            log += "\t<color=red>REMOVING COLLIDER:</color> " + collider.name + "\n";

                            // remove the collider
                            DestroyImmediate(collider, true);
                        }
                    }
                }
                Debug.Log(log);
            }
            else {
                Debug.Log("Nothing selected");
            }
        }
    }
}
