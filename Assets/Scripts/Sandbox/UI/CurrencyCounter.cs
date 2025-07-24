using TMPro;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class CurrencyCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currencyText;

        public void SetCurrencyText(int amount)
        {
            if (_currencyText != null)
            {
                _currencyText.text = amount.ToString();
            }
        }
    }
}

