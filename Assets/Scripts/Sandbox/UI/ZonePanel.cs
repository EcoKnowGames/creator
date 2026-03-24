using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ZonePanel : MonoBehaviour
    {
        [SerializeField] private ZoneKey _zoneKeyPrefab;
        [SerializeField] private RectTransform _zoneKeyContainer;

        public void SetZones(ZoneDef[] zoneDefs)
        {
            if (zoneDefs.Count() == 0 || _zoneKeyContainer == null || _zoneKeyPrefab == null)
            {
                return;
            }

            foreach (Transform child in _zoneKeyContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < zoneDefs.Length; i++)
            {
                ZoneKey key = Instantiate(_zoneKeyPrefab, _zoneKeyContainer);
                if (key != null)
                {
                    key.SetZone(zoneDefs[i]);
                }
            }

            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }
    }
}
