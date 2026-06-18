using UnityEngine;

namespace GPSPanic.Road.Spline
{
    public enum RoadSegmentType
    {
        Straight,
        Merge,
        Split,
        ExitRamp,
        LaneExpansion,
        LaneReduction
    }

    public class RoadSegmentData : MonoBehaviour
    {
        [Header("Connection Points")]
        public Transform entrancePoint;
        public Transform exitPoint;
        public Transform branchExitPoint; // For splits/exits

        [Header("Lane Configuration")]
        public int entranceLaneCount = 3;
        public int exitLaneCount = 3;
        public int branchLaneCount = 0;

        [Header("Type")]
        public RoadSegmentType segmentType = RoadSegmentType.Straight;

        [HideInInspector] public float cachedLength;

        public Vector3 GetExitPosition() => exitPoint.position;
        public Quaternion GetExitRotation() => exitPoint.rotation;

        public void CacheLength()
        {
            var container = GetComponent<UnityEngine.Splines.SplineContainer>();
            if (container != null) cachedLength = container.CalculateLength();
        }
    }
}
