using TMPro;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{

    public class CurrencyPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currencyText;

        public void Refresh()
        {
            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                SetCurrencyText(inventory.GetAmountHeld(PlayerInventory.CurrencyID));
            }
        }

        private void SetCurrencyText(int amount)
        {
            if (_currencyText != null)
            {
                _currencyText.text = amount.ToString();
            }
        }
    }
}

