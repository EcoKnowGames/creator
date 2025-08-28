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

        public void SetPlayer(int index)
        {
            if (_indicatorText != null)
            {
                _indicatorText.text = string.Format($"Player {index + 1}'s Turn"); //Account for 0
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

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            //RectTransform rectTransform = this.GetComponent<RectTransform>();
            if (_indicatorBox != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_indicatorBox);
            }
        }
    }
}
