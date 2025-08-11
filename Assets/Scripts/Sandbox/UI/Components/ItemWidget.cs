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
        [SerializeField] private Image _quantityPanelBackground;

        [Header("Colours")]
        [SerializeField] private Color _validColour;
        [SerializeField] private Color _invalidColour;

        private bool _showSign = false;

        private const string LogChannel = "[ItemWidget]";

        public void Awake()
        {
            SetValid(true);
        }

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

        public void SetQuantity(int quantity, bool forceShowSign = false)
        {
            if (_quantityText != null)
            {
                string quantityStr = FormatQuantity(quantity);
                if ((forceShowSign) && (quantity > 0))
                {
                    quantityStr = "+ " + quantityStr;
                }

                _quantityText.text = quantityStr;
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

        public void SetValid(bool value)
        {
            if (_quantityPanelBackground != null)
            {
                _quantityPanelBackground.color = value == true ? _validColour : _invalidColour;
            }
        }
    }
}

