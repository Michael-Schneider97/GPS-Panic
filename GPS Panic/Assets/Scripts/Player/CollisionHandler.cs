using UnityEngine;
using GPSPanic.Core;

namespace GPSPanic.Player
{
    public class CollisionHandler : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.CompareTag("Traffic"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TakeDamage(1);
                }
                
                Debug.Log($"Collision with {collision.gameObject.name}!");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Exit"))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.IncrementStreak();
                }
                
                Debug.Log("Successfully executed exit!");
            }
        }
    }
}
