using Glitchers.EcoKnow.Sandbox.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.Grid
{
    public class Cell_Token : MonoBehaviour
    {
        private int _entityIndex;
        public int Index => _entityIndex;

        [SerializeField] private TMP_Text _populationText;

        [Header("Icon")]
        [SerializeField] private Image _entityIcon;
        [SerializeField] private Color _populatedColour;
        [SerializeField] private Color _extinctColour;

        [Header("Borders")]
        [SerializeField] private Transform _borderContainer;
        [SerializeField] private GameObject _extinctBorder;
        [SerializeField] private GameObject _vulnerableBorder;
        [SerializeField] private GameObject _stableBorder;
        [SerializeField] private GameObject _abundantBorder;

        private GameObject _currentBorder = null;

        public void Init(int index, Entity type)
        {
            _entityIndex = index;
            SetIcon(type.Icon);
            SetState(CellEntity.State.STABLE);
            UpdateBorderColour(type.Colour);
        }

        public void SetVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }

        #region Population Total
        public void UpdatePopulation(int population, int total, CellEntity.State state)
        {
            SetState(state);

            if (_populationText != null)
            {
                _populationText.text = population.ToString();
            }

            if (_currentBorder != null)
            {
                float percentage = (float)population / (float)total;
                _currentBorder.GetComponent<PercentageMeter>()?.UpdatePercentage(percentage);
            }
        }

        private void SetPopulationVisible(bool visible)
        {
            if (_populationText != null)
            {
                _populationText.gameObject.SetActive(visible);
            }
        }
        #endregion

        #region Icon
        private void SetIcon(Sprite sprite)
        {
            if ((_entityIcon != null) && (sprite != null))
            {
                _entityIcon.sprite = sprite;
            }
        }

        private void SetIconColour(CellEntity.State state)
        {
            if (_entityIcon != null)
            {
                _entityIcon.color = state == CellEntity.State.EXTINCT ? _extinctColour : _populatedColour;
            }
        }
        #endregion

        #region State/Borders
        private void SetState(CellEntity.State state)
        {
            switch (state)
            {
                case (CellEntity.State.EXTINCT):
                    {
                        _currentBorder = _extinctBorder;
                        break;
                    }
                case (CellEntity.State.VULNERABLE):
                    {
                        _currentBorder = _vulnerableBorder;
                        break;
                    }
                case (CellEntity.State.ABUNDANT):
                    {
                        _currentBorder = _abundantBorder;
                        break;
                    }
                case (CellEntity.State.STABLE):
                default:
                    {
                        _currentBorder = _stableBorder;
                        break;
                    }
            }

            //Setup icon colour and population total visibility
            SetIconColour(state);
            SetPopulationVisible(state != CellEntity.State.EXTINCT);

            //Show correct border
            HideAllBorders();
            _currentBorder.gameObject.SetActive(true);
        }

        private void HideAllBorders()
        {
            if (_borderContainer != null)
            {
                foreach (Transform border in _borderContainer.transform)
                {
                    border.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateBorderColour(Color colour)
        {
            if (_borderContainer != null)
            {
                foreach (TokenBorder border in _borderContainer.GetComponentsInChildren<TokenBorder>(true))
                {
                    border.SetColour(colour);
                }
            }
        }
        #endregion
    }
}
