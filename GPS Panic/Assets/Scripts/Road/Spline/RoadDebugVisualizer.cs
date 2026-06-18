using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace GPSPanic.Road.Spline
{
    public class RoadDebugVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        public bool showSpline = true;
        public bool showSockets = true;
        public bool showNormals = true;
        public Color splineColor = Color.cyan;

        private void OnDrawGizmos()
        {
            var container = GetComponent<SplineContainer>();
            if (container == null) return;

            var spline = container.Spline;
            if (spline == null) return;

            // 1. Draw the Spline Path
            if (showSpline)
            {
                Gizmos.color = splineColor;
                float3 lastPos = container.EvaluatePosition(0);
                for (int i = 1; i <= 20; i++)
                {
                    float3 nextPos = container.EvaluatePosition(i / 20f);
                    Gizmos.DrawLine((Vector3)lastPos, (Vector3)nextPos);
                    lastPos = nextPos;
                }
            }

            // 2. Draw Entrance and Exit Sockets
            if (showSockets)
            {
                var segment = GetComponent<RoadSegmentData>();
                if (segment != null)
                {
                    if (segment.entrancePoint != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(segment.entrancePoint.position, 0.5f);
                        Gizmos.DrawRay(segment.entrancePoint.position, segment.entrancePoint.up * 2f);
                    }
                    if (segment.exitPoint != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(segment.exitPoint.position, 0.5f);
                        Gizmos.DrawRay(segment.exitPoint.position, segment.exitPoint.up * 2f);
                    }
                }
            }

            // 3. Draw Spline Tangents (to catch NaNs)
            if (showNormals)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i <= 4; i++)
                {
                    float t = i / 4f;
                    container.Evaluate(t, out float3 pos, out float3 tangent, out float3 up);
                    Gizmos.DrawRay((Vector3)pos, (Vector3)tangent * 2f);
                }
            }
        }
    }
}
