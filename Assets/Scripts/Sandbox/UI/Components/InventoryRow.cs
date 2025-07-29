using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class InventoryRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _itemName;
        [SerializeField] private TMP_Text _itemQuantity;
        [SerializeField] private TMP_Text _buttonText;

        [SerializeField] private Button _sellButton;

        public void Init(Item itemDef, int amount, UnityAction onSellPressed)
        {
            if (_itemName != null)
            {
                _itemName.text = itemDef.ID;
            }

            if (_itemQuantity != null)
            {
                _itemQuantity.text = amount.ToString();
            }

            if ((_sellButton != null) && (_buttonText != null))
            {
                _sellButton.interactable = itemDef.CanSell;

                if (itemDef.CanSell)
                {
                    _buttonText.text = string.Format($"Sell 1 for {itemDef.Value} Currency");
                }
                else
                {
                    _buttonText.text = "Cannot Sell";
                }


                _sellButton.onClick.AddListener(onSellPressed);
            }

        }
    }
}
