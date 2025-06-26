using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class TokenBorder : MonoBehaviour
    {
        [SerializeField] private Image _innerShape;

        public void SetColour(Color colour)
        {
            if (_innerShape != null)
            {
                _innerShape.color = colour;
            }
        }
    }
}
