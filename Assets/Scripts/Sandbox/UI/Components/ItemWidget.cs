using System;
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
                _quantityText.text = FormatQuantity(quantity);
            }
        }

        private string FormatQuantity(int quantity)
        {
            if (quantity >= 1000000)
            {
                float roundedQuantity = Mathf.Floor(((float)quantity / 100000f) * 10f) / 10f;
                return roundedQuantity.ToString("0.#") + "M";
            }
            else if (quantity >= 1000)
            {
                float roundedQuantity = Mathf.Floor(((float)quantity / 1000f) * 10f) / 10f;
                return roundedQuantity.ToString("0.#") + "k";
            }

            return quantity.ToString();
        }
    }
}

