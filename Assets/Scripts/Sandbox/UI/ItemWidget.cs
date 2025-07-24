using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ItemWidget : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TMP_Text _quantityText;

        private const string LogChannel = "[ItemWidget]";

        public void SetIcon(string spritePath)
        {
            if ((_itemIcon != null) && !string.IsNullOrEmpty(spritePath))
            {
                Sprite resource = Resources.Load<Sprite>(spritePath);
                if (resource != null)
                {
                    _itemIcon.sprite = resource;
                }
                else
                {
                    Debug.LogError($"{LogChannel} Failed to find icon for item at path {spritePath}!");
                }
            }
        }

        public void SetQuantity(int quantity)
        {
            if (_quantityText != null)
            {
                //TODO(caspar): Formatting for large numbers
                _quantityText.text = quantity.ToString();
            }
        }
    }
}

