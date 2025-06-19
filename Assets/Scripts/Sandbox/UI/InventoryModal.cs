using System;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class InventoryModal : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _currency;
        [SerializeField] private InventoryRow _inventoryRowPrefab;
        [SerializeField] private RectTransform _inventoryListContent;

        private void Start()
        {
            HideModal();
        }

        public void ShowModal()
        {
            this.gameObject.SetActive(true);
            RefreshInventory();
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        public void OnSellPressed(string id)
        {
            //Sell 1 for amount
            SandboxManager.Instance.PlayerInventory?.SellItem(id, 1);
            RefreshInventory();
        }

        private void RefreshInventory()
        {

            foreach(InventoryRow child in _inventoryListContent.GetComponentsInChildren<InventoryRow>())
            {
                Destroy(child.gameObject);
            }

            if (_inventoryRowPrefab == null)
            {
                return;
            }

            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                //Currency
                if (_currency != null)
                {
                    _currency.text = string.Format($"£{inventory.GetAmountHeld(PlayerInventory.CurrencyID)}");
                }

                //Items
                foreach (Tuple<Item, int> item in inventory.GetItemInventory())
                {
                    InventoryRow row = Instantiate(_inventoryRowPrefab, _inventoryListContent);
                    if (row != null)
                    {
                        row.Init(item.Item1, item.Item2, delegate { OnSellPressed(item.Item1.ID); } );
                    }
                }
            }

            //Temp -> This whole UI will be revamped in the future but this works for now to prevent issues with layouts
            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }
    }
}
