using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class MultiplayerBorder : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup => this.GetComponent<CanvasGroup>();
        [SerializeField] private RectTransform _indicatorBox;
        [SerializeField] private TMP_Text _indicatorText;
        [SerializeField] private Image _borderImage;

        public void SetPlayer(string playerName)
        {
            if (_indicatorText != null)
            {
                //_indicatorText.text = string.Format($"Player {index + 1}'s Turn"); //Account for 0
                _indicatorText.text = string.Format($"{playerName}'s Turn");
            }

            StartCoroutine(RefreshLayout());
        }

        public void SetBorderVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        public void SetBorderColour(Color colour)
        {
            Image indicatorImage = _indicatorBox.GetComponent<Image>();
            if (indicatorImage != null)
            {
                indicatorImage.color = colour;
            }

            if (_borderImage != null)
            {
                _borderImage.color = colour;
            }
        }


        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            if (_indicatorBox != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_indicatorBox);
            }
        }
    }
}
