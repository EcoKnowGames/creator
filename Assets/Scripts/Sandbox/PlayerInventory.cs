using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public record Item(
        string ID,
        Sprite Icon,
        int Value,
        bool CanSell
    );

    public class InventoryItem
    {
        private string _id;
        private int _amountHeld;

        public string ID => _id;
        public int AmountHeld { get {return _amountHeld;} set {_amountHeld = value;}}


        public InventoryItem(string id, int amount)
        {
            _id = id;
            _amountHeld = amount;
        }
    }

    public class PlayerInventory : MonoBehaviour
    {
        private List<Item> _itemDefs = new List<Item>();
        private List<InventoryItem> _inventory = new List<InventoryItem>();

        public const string CurrencyID = "currency"; //This currency is constant between all games and not dictated by a node
        public const string ActionID = "action"; //This currency is constant between all games and not dictated by a node
        private const string LogChannel = "[PlayerInventory]";

        public void RegisterItemDefinitions(List<Item> items)
        {
            _itemDefs = items;
        }

        public int AddItem(string id, int amount)
        {
            if (_inventory == null)
            {
                Debug.LogError($"{LogChannel} Cannot add item with id [{id}], inventory list is null!");
                return 0;
            }

            int amountHeld = 0;

            InventoryItem item = _inventory.FirstOrDefault(x => x.ID.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.AmountHeld += amount;
                amountHeld = item.AmountHeld;
            }
            else
            {
                _inventory.Add(new InventoryItem(id.ToLower(), amount));
                amountHeld = amount;
            }

            return amountHeld;
        }

        public int RemoveItem(string id, int amount)
        {
            if (_inventory == null)
            {
                Debug.LogError($"{LogChannel} Cannot remove item with id [{id}], inventory list is null!");
                return 0;
            }

            int amountHeld = 0;

            InventoryItem item = _inventory.FirstOrDefault(x => x.ID.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.AmountHeld -= amount;
                if (item.AmountHeld <= 0)
                {
                    _inventory.Remove(item);
                    amountHeld = 0;
                }
                else
                {
                    amountHeld = item.AmountHeld;
                }
            }
            else
            {
                //No item found
                Debug.LogError($"{LogChannel} Cannot remove item with id [{id}], none exist in the player's inventory!");
                return 0;
            }

            return amountHeld;
        }

        public int GetAmountHeld(string id)
        {
            if (_inventory == null)
            {
                Debug.LogError($"{LogChannel} Cannot get item quantity with id [{id}], inventory list is null!");
                return 0;
            }

            InventoryItem item = _inventory.FirstOrDefault(x => x.ID.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.AmountHeld;
            }

            return 0;
        }
    }
}
