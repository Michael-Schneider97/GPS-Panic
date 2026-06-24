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
        [Tooltip("Max horizontal shift per segment (higher = sharper curves)")]
        public float maxTurnCurvature = 10f; 
        [Tooltip("How gradually the road changes direction (0 = purely random, 1 = perfectly straight)")]
        [Range(0, 1)] public float headingSmoothness = 0.5f; 
        [Range(0, 1)] public float laneChangeChance = 0.2f;

        public List<RoadSegmentData> activeSegments = new List<RoadSegmentData>();
        private Vector3 nextSpawnPosition = Vector3.zero;
        private Quaternion nextSpawnRotation = Quaternion.identity;

        // Persistent heading state for C1 continuity
        private float currentLocalTurnOffset = 0f;

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

            // 1. Instantiate at previous exit
            RoadSegmentData segment = Instantiate(genericTemplate, nextSpawnPosition, nextSpawnRotation, transform);

            // 2. Align entrance
            if (segment.entrancePoint != null)
            {
                Vector3 localEntrancePos = segment.entrancePoint.localPosition;
                segment.transform.position -= segment.transform.TransformDirection(localEntrancePos);
            }

            // 3. PERSISTENT SMOOTH TURNING
            // Gradually evolve the turn offset to ensure smooth curves over larger timescales
            float targetTurn = Random.Range(-maxTurnCurvature, maxTurnCurvature);
            currentLocalTurnOffset = Mathf.Lerp(currentLocalTurnOffset, targetTurn, 1.0f - headingSmoothness);
            
            ApplyCurvature(segment, currentLocalTurnOffset);

            // 4. Mesh Gen
            if (segment.TryGetComponent<RoadMesh2D>(out var roadMesh))
            {
                roadMesh.Generate2DRoad(startLanes, endLanes);
            }

            segment.CacheLength();
            activeSegments.Add(segment);

            // 5. Save state
            if (segment.exitPoint != null)
            {
                nextSpawnPosition = segment.exitPoint.position;
                nextSpawnRotation = segment.exitPoint.rotation;
                currentLaneCount = endLanes;
            }
        }

        private void ApplyCurvature(RoadSegmentData segment, float turnOffset)
        {
            var container = segment.GetComponent<UnityEngine.Splines.SplineContainer>();
            if (container == null) return;

            var spline = container.Spline;
            if (spline.Count < 2) return;

            float roadLength = spline.GetLength();

            // Knot 1 (Exit): Apply the new evolved turn offset
            var lastKnot = spline[spline.Count - 1];
            lastKnot.Position = new Unity.Mathematics.float3(turnOffset, roadLength, 0);
            
            // BEZIER TANGENTS: Force C1 Continuity
            // We ensure that segments always enter straight (relative to the previous exit)
            // and exit with a tangent that points toward the next segment's intended path.
            float tangentWeight = roadLength * 0.4f;
            
            var firstKnot = spline[0];
            firstKnot.TangentOut = new Unity.Mathematics.float3(0, tangentWeight, 0);
            spline[0] = firstKnot;

            lastKnot.TangentIn = new Unity.Mathematics.float3(0, -tangentWeight, 0);
            spline[spline.Count - 1] = lastKnot;

            // Force sockets to update to the new spline shape
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
