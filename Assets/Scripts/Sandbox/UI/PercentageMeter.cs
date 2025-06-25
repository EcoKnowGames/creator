using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PercentageMeter : MonoBehaviour
    {
        [SerializeField] private Image _percentageMeter;

        public void UpdatePercentage(float percent)
        {
            if (_percentageMeter != null)
            {
                _percentageMeter.fillAmount = percent;
            }
        }
    }
}
