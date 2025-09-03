using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class MultiplayerModal : MonoBehaviour
    {
        [SerializeField] private RectTransform _modalBox;
        [SerializeField] private TMP_Text _newRoundText;
        [SerializeField] private TMP_Text _indicatorText;

        public void SetPlayer(string playerName)
        {
            if (_indicatorText != null)
            {
                //_indicatorText.text = string.Format($"Player {index + 1}'s Turn"); //Account for 0
                _indicatorText.text = string.Format($"{playerName}'s Turn");
            }

            StartCoroutine(RefreshLayout());
        }

        public void ShowModal(string playerName, bool newRound = false)
        {
            SetPlayer(playerName);
            this.gameObject.SetActive(true);

            if (_newRoundText != null)
            {
                _newRoundText.gameObject.SetActive(newRound);
            }
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            if (_modalBox != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_modalBox);
            }
        }
    }
}
