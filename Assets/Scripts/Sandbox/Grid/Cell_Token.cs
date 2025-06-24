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
        [SerializeField] private Image _populationPercentage;
        [SerializeField] private Image _entityIcon;

        public void Init(int index, Entity type)
        {
            _entityIndex = index;
            SetIcon(type.Icon);
        }

        public void UpdatePopulation(int population, int total)
        {
            if (_populationText != null)
            {
                _populationText.text = population.ToString();
            }

            if (_populationPercentage != null)
            {
                float percentage = (float)population / (float)total;
                _populationPercentage.fillAmount = percentage;
            }
        }

        private void SetIcon(Sprite sprite)
        {
            if ((_entityIcon != null) && (sprite != null))
            {
                _entityIcon.sprite = sprite;
            }
        }

        public void SetVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }
    }
}
