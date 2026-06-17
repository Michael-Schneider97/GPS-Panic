using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace GPSPanic.Road.Spline
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineContainer))]
    public class SplineRoadSegment : RoadSegmentData
    {
        private SplineContainer splineContainer;

        private void OnValidate()
        {
            SetupSockets();
        }

        private void SetupSockets()
        {
            if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
            
            // AUTO-FIND: If sockets are not assigned, try to find them by name
            if (entrancePoint == null) entrancePoint = transform.Find("Entrance");
            if (exitPoint == null) exitPoint = transform.Find("Exit");

            var spline = splineContainer.Spline;
            if (spline == null || spline.Count < 2) return;

            // Align Entrance to start of spline (Point 0)
            if (entrancePoint != null)
            {
                float3 pos, tangent, up;
                spline.Evaluate(0, out pos, out tangent, out up);
                
                // Safety check for tangent
                if (math.lengthsq(tangent) < 0.001f) tangent = new float3(0, 1, 0);

                entrancePoint.localPosition = (Vector3)pos;
                entrancePoint.localRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
            }

            // Align Exit to end of spline (Point 1.0)
            if (exitPoint != null)
            {
                float3 pos, tangent, up;
                spline.Evaluate(1.0f, out pos, out tangent, out up);

                // Safety check for tangent
                if (math.lengthsq(tangent) < 0.001f) tangent = new float3(0, 1, 0);

                exitPoint.localPosition = (Vector3)pos;
                exitPoint.localRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
            }
        }

        // Helper to get a point on the road for spawning hazards/traffic
        public Vector3 GetPointAtDistance(float t)
        {
            if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
            return splineContainer.EvaluatePosition(t);
        }
    }
}
