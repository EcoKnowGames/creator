using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class InventoryModal : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _currency;
        [SerializeField] private InventoryRow _inventoryRowPrefab;
        [SerializeField] private RectTransform _inventoryRowContainer;

        [Header("Buttons")]
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _unitText;
        [SerializeField] private TMP_Text _profitText;
        [SerializeField] private Button _cancelButton;

        private InventoryRow[] _inventoryRowList => _inventoryRowContainer == null ? null : _inventoryRowContainer.GetComponentsInChildren<InventoryRow>();

        private void Start()
        {
            HideModal();
        }

        public void ShowModal()
        {
            this.gameObject.SetActive(true);
            RefreshInventory();
            OnUnitsAdjusted();
        }

        public void HideModal()
        {
            OnCancelPressed();
            this.gameObject.SetActive(false);
        }

        public void OnSellPressed()
        {
            if (_inventoryRowList != null)
            {
                if (_inventoryRowList.Count() > 0)
                {
                    foreach (InventoryRow row in _inventoryRowList)
                    {
                        SandboxManager.Instance.PlayerInventory?.SellItem(row.ItemID, row.SelectedUnits);
                        row.ClearSelection();
                    }

                    RefreshInventory();
                    OnUnitsAdjusted();
                }
            }
        }

        public void OnCancelPressed()
        {
            if (_inventoryRowList != null)
            {
                foreach (InventoryRow row in _inventoryRowList)
                {
                    row.ClearSelection();
                }

                RefreshInventory();
                OnUnitsAdjusted();
            }
        }

        public void OnUnitsAdjusted()
        {
            bool anySelected = false;
            int totalUnits = 0;
            int totalProfit = 0;
            foreach (InventoryRow row in _inventoryRowList)
            {
                if (row.IsSelectedForSell)
                {
                    anySelected = true;
                    totalUnits += row.SelectedUnits;
                    totalProfit += (row.SelectedUnits * row.ItemValue);
                }
            }

            if (_sellButton != null)
            {
                _sellButton.interactable = anySelected;
            }

            if (_unitText != null)
            {
                _unitText.text = string.Format($"Sell {totalUnits} Units for ");
            }

            if (_profitText != null)
            {
                _profitText.text = totalProfit.ToString();
            }
        }

        private void RefreshInventory()
        {

            foreach(InventoryRow child in _inventoryRowContainer.GetComponentsInChildren<InventoryRow>())
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
                    InventoryRow row = Instantiate(_inventoryRowPrefab, _inventoryRowContainer);
                    if (row != null)
                    {
                        row.Init(item.Item1, item.Item2, delegate { OnUnitsAdjusted(); } );
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
