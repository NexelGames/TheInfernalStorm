using UnityEngine;

namespace Extensions.Mono {
	[AddComponentMenu("Common/Utils/Visualization/Point Debug")]
    public class DebugPoint : MonoBehaviour {
        public float Radius;
        public Color Color = Color.red;

        private void OnDrawGizmos() {
            Gizmos.color = Color;
            Gizmos.DrawSphere(transform.position, Radius);
        }
    }
}