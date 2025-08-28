using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class MultiplayerModal : MonoBehaviour
    {
        [SerializeField] private TMP_Text _indicatorText;
        public void SetPlayer(int index)
        {
            if (_indicatorText != null)
            {
                _indicatorText.text = string.Format($"Player {index + 1}'s Turn"); //Account for 0
            }
        }

        public void ShowModal(int index)
        {
            SetPlayer(index);
            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }
    }
}
