using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("Item Widgets")]
        [SerializeField] private int _widgetCount = 4;
        [SerializeField] private Transform _widgetContainer;
        [SerializeField] ItemWidget[] _enabledWidgets;
        [SerializeField] ItemWidget[] _disabledWidgets;

        public void RefreshInventory()
        {
            DisableWidgets();

            if ((_enabledWidgets == null) || (_disabledWidgets == null)
                || (_enabledWidgets.Count() <= 0) || (_disabledWidgets.Count() <= 0))
            {
                return;
            }

            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                List<Tuple<Item, int>> sortedInventory = inventory.GetItemInventory()
                    .OrderByDescending(x => x.Item2)
                    .Where(x => !x.Item1.ID.Equals(PlayerInventory.CurrencyID) && !x.Item1.ID.Equals(PlayerInventory.ActionID))
                    .ToList();

                for (int i = 0; i < 4; i++)
                {
                    if (i < sortedInventory.Count)
                    {
                        Tuple<Item, int> item = sortedInventory[i];
                        ItemWidget widget = _enabledWidgets[i];

                        //Show our item in a widget
                        widget.SetIcon(item.Item1.Icon);
                        widget.SetQuantity(item.Item2);
                        widget.gameObject.SetActive(true);
                    }
                    else
                    {
                        //Activate a disabled widget
                        ItemWidget widget = _disabledWidgets[i];
                        widget.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void DisableWidgets()
        {
            foreach(ItemWidget widget in _enabledWidgets)
            {
                widget.gameObject.SetActive(false);
            }

            foreach (ItemWidget widget in _disabledWidgets)
            {
                widget.gameObject.SetActive(false);
            }
        }
    }
}

