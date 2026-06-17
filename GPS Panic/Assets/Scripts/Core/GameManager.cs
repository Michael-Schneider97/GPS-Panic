using UnityEngine;
using System;

namespace GPSPanic.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;

        [Header("Scoring Settings")]
        [SerializeField] private float baseDistancePoints = 10f;
        private float currentScore = 0f;
        private float laneMultiplier = 1.0f;
        private float streakMultiplier = 1.0f;

        public event Action<int> OnHealthChanged;
        public event Action<float> OnScoreChanged;
        public event Action OnGameOver;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }

        private void Update()
        {
            if (currentHealth > 0)
            {
                UpdateScore();
            }
        }

        private void UpdateScore()
        {
            // Score Per Second = Base Distance Points × Mlane × Mstreak
            float pointsThisFrame = baseDistancePoints * laneMultiplier * streakMultiplier * Time.deltaTime;
            currentScore += pointsThisFrame;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            OnHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                GameOver();
            }

            // GDD: Resets streak multiplier back to 1.0x instantly
            ResetStreak();
        }

        public void UpdateLaneMultiplier(float multiplier)
        {
            laneMultiplier = multiplier;
        }

        public void IncrementStreak()
        {
            // GDD: Each successful execution... increments this multiplier by +0.5x
            streakMultiplier += 0.5f;
        }

        public void ResetStreak()
        {
            streakMultiplier = 1.0f;
        }

        private void GameOver()
        {
            Debug.Log("Game Over: You Have Arrived at Your Destination.");
            OnGameOver?.Invoke();
        }
    }
}
