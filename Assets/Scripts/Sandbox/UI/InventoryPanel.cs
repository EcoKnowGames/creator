using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class InventoryPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currency;
        [SerializeField] private TMP_Text _inventoryList;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void RefreshInventory()
        {
            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                _currency.text = string.Format($"£{inventory.GetAmountHeld(PlayerInventory.CurrencyID)}");
    

                _inventoryList.text = string.Empty;
                foreach(Tuple<Item, int> item in inventory.GetItemInventory())
                {
                    _inventoryList.text += string.Format($"{item.Item1.ID}: {item.Item2}\n");
                }
            }

            //Temp -> This whole UI will be revamped in the future but this works for now to prevent issues with layouts
            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
            if (this.GetComponentInParent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponentInParent<RectTransform>());
            }
        }
    }
}

