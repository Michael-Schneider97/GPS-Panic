using UnityEngine;
using GPSPanic.Input;
using System.Collections;

namespace GPSPanic.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 10f;
        [SerializeField] private float speedScaleRate = 0.1f;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private int totalLanes = 3;
        [SerializeField] private float laneChangeSpeed = 15f;
        [SerializeField] private float steeringSensitivity = 0.01f;

        [Header("Lane Speed Multipliers")]
        [SerializeField] private float[] laneSpeedMultipliers = { 1.0f, 1.5f, 3.0f };

        private int currentLane = 1; // Middle lane (0, 1, 2)
        private float currentHorizontalPosition = 0f;
        private float currentVerticalSpeed = 0f;
        private float ambientSpeedScale = 1.0f;
        private bool isChangingLane = false;
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

            currentHorizontalPosition = GetLaneXPosition(currentLane);
            transform.position = new Vector3(currentHorizontalPosition, 0f, 0f);
            currentVerticalSpeed = baseSpeed;
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
            UpdateSpeed();
            UpdateMovement();
        }

        private void UpdateSpeed()
        {
            // GDD: Ambient speed scales up slowly over time
            ambientSpeedScale += speedScaleRate * Time.deltaTime;

            // GDD: Determined by individual lane occupied
            int clampedLane = Mathf.Clamp(currentLane, 0, laneSpeedMultipliers.Length - 1);
            float targetSpeed = baseSpeed * ambientSpeedScale * laneSpeedMultipliers[clampedLane];
            currentVerticalSpeed = Mathf.Lerp(currentVerticalSpeed, targetSpeed, Time.deltaTime * 2f);

            // Update GameManager multiplier
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.UpdateLaneMultiplier(laneSpeedMultipliers[clampedLane]);
            }
        }

        private void UpdateMovement()
        {
            // Vertical movement (forward)
            transform.Translate(Vector3.up * currentVerticalSpeed * Time.deltaTime);

            // Horizontal movement
            if (!isDragging && !isChangingLane)
            {
                // Snap back to lane center
                float targetX = GetLaneXPosition(currentLane);
                currentHorizontalPosition = Mathf.Lerp(currentHorizontalPosition, targetX, Time.deltaTime * laneChangeSpeed);
            }

            // Boundary clamping for arcade steering
            float halfTotalWidth = (totalLanes * laneWidth) / 2f;
            currentHorizontalPosition = Mathf.Clamp(currentHorizontalPosition, -halfTotalWidth, halfTotalWidth);

            transform.position = new Vector3(currentHorizontalPosition, transform.position.y, transform.position.z);
        }

        private void HandleSwipeLeft()
        {
            if (currentLane > 0)
            {
                currentLane--;
                StartCoroutine(ChangeLaneRoutine(GetLaneXPosition(currentLane)));
            }
        }

        private void HandleSwipeRight()
        {
            if (currentLane < totalLanes - 1)
            {
                currentLane++;
                StartCoroutine(ChangeLaneRoutine(GetLaneXPosition(currentLane)));
            }
        }

        private void HandleDragDelta(Vector2 delta)
        {
            isDragging = true;
            // GDD: Manual micro-steering control, overrides lane locking
            currentHorizontalPosition += delta.x * steeringSensitivity;
        }

        private void HandleTouchReleased()
        {
            isDragging = false;
            // GDD: Releasing the touch smoothly glides the vehicle into the nearest defined lane center
            currentLane = Mathf.RoundToInt((currentHorizontalPosition / laneWidth) + (totalLanes - 1) / 2f);
            currentLane = Mathf.Clamp(currentLane, 0, totalLanes - 1);
        }

        private float GetLaneXPosition(int laneIndex)
        {
            return (laneIndex - (totalLanes - 1) / 2f) * laneWidth;
        }

        private IEnumerator ChangeLaneRoutine(float targetX)
        {
            isChangingLane = true;
            float startX = currentHorizontalPosition;
            float t = 0;
            while (t < 1.0f)
            {
                t += Time.deltaTime * laneChangeSpeed;
                currentHorizontalPosition = Mathf.Lerp(startX, targetX, t);
                yield return null;
            }
            currentHorizontalPosition = targetX;
            isChangingLane = false;
        }
    }
}
