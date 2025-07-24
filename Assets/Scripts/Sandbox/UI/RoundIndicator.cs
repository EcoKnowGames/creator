using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class RoundIndicator : MonoBehaviour
    {
        [SerializeField] private TMP_Text _roundText;

        [Header("Colours")]
        [SerializeField] private Color _currentColour;
        [SerializeField] private Color _maxColour;

        public void UpdateRoundCounter(int currentRound, int maxRounds)
        {
            if (_roundText != null)
            {
                string currentStr = string.Format($"<color=#{ColorUtility.ToHtmlStringRGB(_currentColour)}>{currentRound + 1}</color>");
                string maxStr = string.Format($"<color=#{ColorUtility.ToHtmlStringRGB(_maxColour)}>/{maxRounds}</color>");

                _roundText.text = currentStr + maxStr;
            }
        }
    }
}
