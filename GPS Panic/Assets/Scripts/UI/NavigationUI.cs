using UnityEngine;
using TMPro;
using System.Collections;

namespace GPSPanic.UI
{
    public class NavigationUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private float instructionInterval = 5f;

        private string[] sampleInstructions = {
            "In 300 feet, keep right for Exit 14B...",
            "Correction: Keep left for Exit 14A!",
            "Recalculating...",
            "Follow signs for Downtown",
            "Slight right onto Main St",
            "U-turn where possible"
        };

        private void Start()
        {
            if (instructionText != null)
            {
                StartCoroutine(UpdateInstructionsRoutine());
            }
        }

        private IEnumerator UpdateInstructionsRoutine()
        {
            while (true)
            {
                string instruction = sampleInstructions[Random.Range(0, sampleInstructions.Length)];
                instructionText.text = instruction;
                
                if (instruction.Contains("Correction") || instruction.Contains("Recalculating"))
                {
                    // Trigger jarring effect as per GDD
                    FlashText();
                }

                yield return new WaitForSeconds(instructionInterval);
            }
        }

        private void FlashText()
        {
            // Simple flash effect could be implemented here
            Debug.Log("Jarring UI effect triggered!");
        }
    }
}
