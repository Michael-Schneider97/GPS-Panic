using UnityEngine;
using System.Collections.Generic;

namespace GPSPanic.Road.Spline
{
    public class SplineRoadGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        public RoadSegmentData[] straightPrefabs;
        public RoadSegmentData[] transitionPrefabs;
        public RoadSegmentData[] exitPrefabs;

        [Header("Generation Settings")]
        public Transform playerTransform;
        public float spawnAheadDistance = 200f;
        public int initialSegments = 15;

        private List<RoadSegmentData> activeSegments = new List<RoadSegmentData>();
        private Vector3 nextSpawnPosition = Vector3.zero;
        private Quaternion nextSpawnRotation = Quaternion.identity;
        private int currentLaneCount = 3;

        private void Start()
        {
            // Initial burst of road
            for (int i = 0; i < initialSegments; i++)
            {
                GenerateNextSegment();
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Trigger next segment based on distance to the end of the current road
            if (Vector3.Distance(playerTransform.position, nextSpawnPosition) < spawnAheadDistance)
            {
                GenerateNextSegment();
                
                // Cleanup old road behind the player
                if (activeSegments.Count > initialSegments + 5)
                {
                    RoadSegmentData old = activeSegments[0];
                    activeSegments.RemoveAt(0);
                    Destroy(old.gameObject);
                }
            }
        }

        private void GenerateNextSegment()
        {
            RoadSegmentData prefab = SelectNextPrefab();
            if (prefab == null) return;

            // Instantiate at the last exit's position and rotation
            RoadSegmentData segment = Instantiate(prefab, nextSpawnPosition, nextSpawnRotation, transform);
            
            // SNAPPING: Align segment entrance to the previous exit exactly
            if (segment.entrancePoint != null)
            {
                Vector3 localEntrancePos = segment.entrancePoint.localPosition;
                // Move the segment root so the entrance point overlaps nextSpawnPosition exactly
                segment.transform.position -= segment.transform.TransformDirection(localEntrancePos);
            }

            // DYNAMIC MESH: Build the 2D vector look
            if (segment.TryGetComponent<RoadMesh2D>(out var roadMesh))
            {
                roadMesh.Generate2DRoad(currentLaneCount);
            }

            activeSegments.Add(segment);

            // Save the exit of this new segment for the next one
            if (segment.exitPoint != null)
            {
                nextSpawnPosition = segment.exitPoint.position;
                nextSpawnRotation = segment.exitPoint.rotation;
                currentLaneCount = segment.exitLaneCount;
            }
            else
            {
                Debug.LogError($"Road segment {segment.name} is missing an ExitPoint!");
            }
        }

        private RoadSegmentData SelectNextPrefab()
        {
            // Simplified for reliability: primarily use straights
            if (straightPrefabs == null || straightPrefabs.Length == 0) return null;
            
            List<RoadSegmentData> valid = new List<RoadSegmentData>();
            foreach (var p in straightPrefabs)
            {
                if (p.entranceLaneCount == currentLaneCount) valid.Add(p);
            }

            if (valid.Count > 0) return valid[Random.Range(0, valid.Count)];
            return straightPrefabs[0];
        }
    }
}
