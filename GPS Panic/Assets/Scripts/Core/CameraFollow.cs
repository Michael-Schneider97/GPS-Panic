using UnityEngine;

namespace GPSPanic.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targeting")]
        public Transform target;
        
        [Header("Settings")]
        [SerializeField] private float smoothness = 0.125f;
        [SerializeField] private Vector3 offset = new Vector3(0, 4, -10);
        
        // This ensures the camera remains "Up" relative to the player
        [SerializeField] private bool followRotation = true;

        private void LateUpdate()
        {
            if (target == null) return;

            // 1. Calculate the target position based on player's current orientation
            // We want the camera to stay behind and above the player
            Vector3 desiredPosition = target.position + (target.rotation * offset);
            
            // 2. Smoothly move the camera to that position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothness);

            // 3. Handle the "Track-Up" rotation
            if (followRotation)
            {
                // We want the camera's 'Up' to match the player's 'Forward' (since it's 2D)
                // This makes the player always look like they are heading towards the top of the screen
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, smoothness);
            }
        }
    }
}
