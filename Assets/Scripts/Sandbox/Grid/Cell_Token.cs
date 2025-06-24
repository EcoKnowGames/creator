using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class Cell_Token : MonoBehaviour
    {
        private int _entityIndex;
        public int Index => _entityIndex;


        [SerializeField] private TMP_Text _populationText;
        [SerializeField] private Image _entityIcon;

        public void Init(int index, Entity type)
        {
            _entityIndex = index;
            SetIcon(type.Icon);
        }

        public void UpdatePopulation(int population)
        {
            if (_populationText != null)
            {
                _populationText.text = population.ToString();
            }
        }

        private void SetIcon(Sprite sprite)
        {
            if ((_entityIcon != null) && (sprite != null))
            {
                _entityIcon.sprite = sprite;
            }
        }
    }
}
