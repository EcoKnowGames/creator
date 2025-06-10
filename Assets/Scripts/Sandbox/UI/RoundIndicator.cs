using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class RoundIndicator : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundText;

        public void UpdateRoundCounter(int currentRound, int maxRounds)
        {
            if (_roundText != null)
            {
                _roundText.text = string.Format($"Round {currentRound + 1}/{maxRounds}");
            }
        }
    }
}
