using UnityEngine;
using TMPro;
using GPSPanic.Core;

namespace GPSPanic.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text healthText;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnHealthChanged += UpdateHealth;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnHealthChanged -= UpdateHealth;
            }
        }

        private void UpdateScore(float score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        }

        private void UpdateHealth(int health)
        {
            if (healthText != null)
            {
                // GDD: Mimicking device telemetry (e.g., Network Signal Bars)
                healthText.text = $"Signal: {new string('|', Mathf.Max(0, health))}";
            }
        }
    }
}
