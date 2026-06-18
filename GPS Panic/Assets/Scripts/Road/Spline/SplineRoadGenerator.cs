using UnityEngine;
using System.Collections.Generic;

namespace GPSPanic.Road.Spline
{
    public class SplineRoadGenerator : MonoBehaviour
    {
        [Header("Template Prefabs")]
        [Tooltip("A generic 1-spline segment for Straights, Curves, and Lane Math.")]
        public RoadSegmentData genericTemplate;
        [Tooltip("A 2-spline segment for Exit Ramps.")]
        public RoadSegmentData exitTemplate;

        [Header("Generation Settings")]
        public Transform playerTransform;
        public float spawnAheadDistance = 250f;
        public int initialSegments = 10;

        [Header("Highway Logic")]
        public int currentLaneCount = 3;
        public float curveIntensity = 15f;
        public float maxTurnAngle = 25f; // Max degrees the exit can deviate from the entrance
        [Range(0, 1)] public float laneChangeChance = 0.3f;
        [Range(0, 1)] public float turnChance = 0.7f;

        public List<RoadSegmentData> activeSegments = new List<RoadSegmentData>();
        private Vector3 nextSpawnPosition = Vector3.zero;
        private Quaternion nextSpawnRotation = Quaternion.identity;

        // Track the current 'heading' to ensure smooth continuity
        private float currentWorldHeading = 0f;

        private void Start()
        {
            for (int i = 0; i < initialSegments; i++)
            {
                GenerateNextSegment();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            if (Vector3.Distance(playerTransform.position, nextSpawnPosition) < spawnAheadDistance)
            {
                GenerateNextSegment();
                CleanupOldSegments();
            }
        }

        private void GenerateNextSegment()
        {
            if (genericTemplate == null) return;

            int startLanes = currentLaneCount;
            int endLanes = currentLaneCount;

            if (Random.value < laneChangeChance)
            {
                int change = Random.value > 0.5f ? 1 : -1;
                endLanes = Mathf.Clamp(startLanes + change, 1, 7);
            }

            // Instantiate with the current world rotation
            RoadSegmentData segment = Instantiate(genericTemplate, nextSpawnPosition, nextSpawnRotation, transform);

            if (segment.entrancePoint != null)
            {
                Vector3 localEntrancePos = segment.entrancePoint.localPosition;
                segment.transform.position -= segment.transform.TransformDirection(localEntrancePos);
            }

            // SMOOTH CURVATURE: Apply Bézier math
            if (Random.value < turnChance)
            {
                ApplySmoothTurn(segment);
            }

            if (segment.TryGetComponent<RoadMesh2D>(out var roadMesh))
            {
                roadMesh.Generate2DRoad(startLanes, endLanes);
            }

            // CACHE LENGTH for locomotion stability
            segment.CacheLength();

            activeSegments.Add(segment);

            if (segment.exitPoint != null)
            {
                nextSpawnPosition = segment.exitPoint.position;
                nextSpawnRotation = segment.exitPoint.rotation;
                currentLaneCount = endLanes;
            }
        }

        private void ApplySmoothTurn(RoadSegmentData segment)
        {
            var container = segment.GetComponent<UnityEngine.Splines.SplineContainer>();
            if (container == null) return;

            var spline = container.Spline;
            if (spline.Count < 2) return;

            // We manipulate the last point to create a curve
            // For a smooth road, we want the entrance tangent to be purely 'Forward' (local Y)
            // and the exit tangent to be our new desired direction.
            
            float turnAngle = Random.Range(-maxTurnAngle, maxTurnAngle);
            float roadLength = spline.GetLength();
            
            // Calculate new local position for the exit knot
            // Assuming local Y is forward and local X is right/left
            float rad = turnAngle * Mathf.Deg2Rad;
            float targetX = Mathf.Sin(rad) * roadLength;
            float targetY = Mathf.Cos(rad) * roadLength;

            var lastKnot = spline[spline.Count - 1];
            lastKnot.Position = new Unity.Mathematics.float3(targetX, targetY, 0);
            
            // Set Tangents for smooth Bézier curve (C1 Continuity)
            // In knot: Handle coming into the point
            // Out knot: Handle leaving the point
            float tangentStrength = roadLength * 0.5f;
            
            // Start Knot: Force straight exit
            var firstKnot = spline[0];
            firstKnot.TangentOut = new Unity.Mathematics.float3(0, tangentStrength, 0);
            spline[0] = firstKnot;

            // End Knot: Angled entry
            lastKnot.TangentIn = new Unity.Mathematics.float3(Mathf.Sin(rad) * -tangentStrength, -Mathf.Cos(rad) * tangentStrength, 0);
            spline[spline.Count - 1] = lastKnot;

            // Update sockets
            segment.SendMessage("SetupSockets", SendMessageOptions.DontRequireReceiver);
        }

        private void CleanupOldSegments()
        {
            if (activeSegments.Count > initialSegments + 5)
            {
                RoadSegmentData old = activeSegments[0];
                activeSegments.RemoveAt(0);
                Destroy(old.gameObject);
            }
        }
    }
}
