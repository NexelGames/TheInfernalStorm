using UnityEngine;

namespace Extensions.Mono {
	[AddComponentMenu("Common/Utils/Visualization/Line Debug")]
    public class DebugLineForward : MonoBehaviour {
        public float LineLength = 20f;
        public Color Color = Color.red;

        private void OnDrawGizmos() {
            Gizmos.color = Color;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * LineLength);
        }
    }
}