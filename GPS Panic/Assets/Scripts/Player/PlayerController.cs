using UnityEngine;
using GPSPanic.Input;
using GPSPanic.Road.Spline;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections;

namespace GPSPanic.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 15f;
        [SerializeField] private float speedScaleRate = 0.05f;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private int totalLanes = 3;
        [SerializeField] private float laneChangeSpeed = 15f;
        [SerializeField] private float steeringSensitivity = 0.01f;

        [Header("References")]
        [SerializeField] private SplineRoadGenerator roadGenerator;

        private int currentLane = 1; 
        private float currentHorizontalOffset = 0f;
        private float currentVerticalSpeed = 0f;
        private float ambientSpeedScale = 1.0f;
        
        // STABLE TRACKING: Use object reference instead of volatile index
        private RoadSegmentData currentSegment;
        private float distanceInCurrentSegment = 0f;
        private bool isDragging = false;

        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSwipeLeft += HandleSwipeLeft;
                InputManager.Instance.OnSwipeRight += HandleSwipeRight;
                InputManager.Instance.OnDragDelta += HandleDragDelta;
                InputManager.Instance.OnTouchReleased += HandleTouchReleased;
            }

            currentVerticalSpeed = baseSpeed;
            if (roadGenerator == null) roadGenerator = FindFirstObjectByType<SplineRoadGenerator>();
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSwipeLeft -= HandleSwipeLeft;
                InputManager.Instance.OnSwipeRight -= HandleSwipeRight;
                InputManager.Instance.OnDragDelta -= HandleDragDelta;
                InputManager.Instance.OnTouchReleased -= HandleTouchReleased;
            }
        }

        private void Update()
        {
            if (roadGenerator == null || roadGenerator.activeSegments.Count == 0) return;

            // Initialize current segment if starting
            if (currentSegment == null) currentSegment = roadGenerator.activeSegments[0];

            UpdateSpeed();
            UpdateSplineMovement();
        }

        private void UpdateSpeed()
        {
            ambientSpeedScale += speedScaleRate * Time.deltaTime;
            currentVerticalSpeed = baseSpeed * ambientSpeedScale;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.6f);
            Gizmos.DrawRay(transform.position, transform.up * 3f);
        }

        private void UpdateSplineMovement()
        {
            // 1. Progress distance
            distanceInCurrentSegment += currentVerticalSpeed * Time.deltaTime;

            // 2. Check for transition
            // Using cachedLength for stability and performance
            if (distanceInCurrentSegment >= currentSegment.cachedLength)
            {
                TransitionToNextSegment();
            }

            // 3. Evaluate Position
            var splineContainer = currentSegment.GetComponent<SplineContainer>();
            float normalizedT = Mathf.Clamp01(distanceInCurrentSegment / currentSegment.cachedLength);
            splineContainer.Evaluate(normalizedT, out float3 worldPos, out float3 tangent, out float3 up);

            // 4. Horizontal Offset
            if (!isDragging)
            {
                float targetX = (currentLane - (totalLanes - 1) / 2f) * laneWidth;
                currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, targetX, Time.deltaTime * laneChangeSpeed);
            }

            // 5. Final Transform
            float3 right = math.normalize(math.cross(tangent, new float3(0, 0, 1)));
            transform.position = (Vector3)worldPos + (Vector3)(right * currentHorizontalOffset);
            
            if (math.lengthsq(tangent) > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, (Vector3)tangent);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }

        private void TransitionToNextSegment()
        {
            int currentIndex = roadGenerator.activeSegments.IndexOf(currentSegment);
            
            if (currentIndex >= 0 && currentIndex < roadGenerator.activeSegments.Count - 1)
            {
                // Hand off to the next piece in the generator's list
                distanceInCurrentSegment -= currentSegment.cachedLength;
                currentSegment = roadGenerator.activeSegments[currentIndex + 1];
            }
            else
            {
                // We've reached the very end of all generated road (shouldn't happen with spawnAheadDistance)
                distanceInCurrentSegment = currentSegment.cachedLength;
            }
        }

        private void HandleSwipeLeft() { if (currentLane > 0) currentLane--; }
        private void HandleSwipeRight() { if (currentLane < totalLanes - 1) currentLane++; }

        private void HandleDragDelta(Vector2 delta)
        {
            isDragging = true;
            currentHorizontalOffset += delta.x * steeringSensitivity;
            float maxHalfWidth = (totalLanes * laneWidth) / 2f;
            currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxHalfWidth, maxHalfWidth);
        }

        private void HandleTouchReleased()
        {
            isDragging = false;
            currentLane = Mathf.RoundToInt((currentHorizontalOffset / laneWidth) + (totalLanes - 1) / 2f);
            currentLane = Mathf.Clamp(currentLane, 0, totalLanes - 1);
        }
    }
}
