using UnityEngine;

namespace Extensions.Mono {
	[AddComponentMenu("Common/Utils/Visualization/WireArc Debug")]
    public class WireArcGizmoDrawer : MonoBehaviour {
        [SerializeField]
        private float _viewZoneAngle = 60f;
        [SerializeField]
        private float _viewRadius = 5f;

        private void OnDrawGizmos() {
            GizmosExtensions.DrawWireArc(transform.position, _viewRadius, _viewZoneAngle, 20, transform.rotation * Quaternion.AngleAxis(-_viewZoneAngle * 0.5f, Vector3.up));
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-_viewZoneAngle * 0.5f, Vector3.up) * transform.forward * _viewRadius);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(_viewZoneAngle * 0.5f, Vector3.up) * transform.forward * _viewRadius);
        }
    }
}