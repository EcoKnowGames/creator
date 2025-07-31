using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class EntityWidget : MonoBehaviour
    {
        public enum State { DEFAULT, FOCUSED };
        private State _currentState;

        private int _entityIndex; //Safety for selection/deselection, as we re-order the list
        public int EntityIndex => _entityIndex;

        [SerializeField] private Button _button;
        public Button Button => _button;

        [Header("UI Elements")]
        [SerializeField] private Image _entityIcon;
        [SerializeField] private Image _entityBackground;
        [SerializeField] private Image _quantityBackground;
        [SerializeField] private TMP_Text _quantityText;

        [Header("Colours")]
        [SerializeField] private Color _defaultColour;
        [SerializeField] private Color _focusedColour;

        private const string LogChannel = "[EntityWidget]";

        #region Visuals
        public void SetEntity(int index, Entity entity)
        {
            _entityIndex = index;

            //Set icon
            if (_entityIcon != null)
            {
                Sprite resource = Resources.Load<Sprite>(entity.Icon);
                if (resource != null)
                {
                    _entityIcon.sprite = resource;
                }
                else
                {
                    Debug.LogError($"{LogChannel} Failed to find icon for entity at path {entity.Icon}!");
                }
            }

            //Set Colour
            if (_entityBackground != null)
            {
                Color colour = Color.white;
                ColorUtility.TryParseHtmlString("#" + entity.Colour, out colour);
                _entityBackground.color = colour;
            }

            //Update populations
            UpdateQuantity();
        }

        public void UpdateQuantity()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(_entityIndex);
                if (_quantityText != null)
                {
                    _quantityText.text = FormatQuantity(population);
                }
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
        #endregion

        #region State
        public void Focus()
        {
            SetState(State.FOCUSED);
        }

        public void Unfocus()
        {
            SetState(State.DEFAULT);
        }

        private void SetState(State state)
        {
            _currentState = state;
            switch (state)
            {
                case (State.FOCUSED):
                    {
                        if (_quantityBackground != null)
                        {
                            _quantityBackground.color = _focusedColour;
                        }
                        break;
                    }
                case (State.DEFAULT):
                default:
                    {
                        if (_quantityBackground != null)
                        {
                            _quantityBackground.color = _defaultColour;
                        }
                        break;
                    }
            }
        }
        #endregion
    }
}
