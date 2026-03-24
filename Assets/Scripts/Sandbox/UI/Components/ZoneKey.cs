using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ZoneKey : MonoBehaviour
    {
        [SerializeField] private Image _swatchImage;
        [SerializeField] private TMP_Text _zoneNameText;

        public void SetZone(ZoneDef zone)
        {
            if (_swatchImage != null)
            {
                if (ColorUtility.TryParseHtmlString(zone.Colour, out Color color))
                {
                    _swatchImage.color = color;
                }
            }

            if (_zoneNameText != null)
            {
                _zoneNameText.text = zone.Name;
            }
        }
    }
}
