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

        public void Init(int index)
        {
            _entityIndex = index;
        }

        public void UpdatePopulation(int population)
        {
            if (_populationText != null)
            {
                _populationText.text = population.ToString();
            }
        }
    }
}
