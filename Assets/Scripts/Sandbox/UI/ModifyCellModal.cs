using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ModifyCellModal : MonoBehaviour
    {
        [Header("Entity Info")]
        [SerializeField] private TMP_Text _selectedEntityText;
        [SerializeField] private EntityIcon _entityIcon;
        [SerializeField] private TMP_Text _currentPopulationText;
        [SerializeField] private TMP_Text _predictedPopulationText;

        [Header("Text")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _confirmText;
        [SerializeField] private TMP_Text _inventoryChange;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _decreaseButton;
        [SerializeField] private Button _increaseButton;
        [SerializeField] private TMP_Dropdown _tileSelectDropdown;

        [Header("Toggles")]
        [SerializeField] private GameObject _unitTypeContainer;
        [SerializeField] private Toggle _discreteToggle;
        [SerializeField] private Toggle _percentToggle;

        [Header("Items")]
        [SerializeField] private GameObject _itemChangeContainer;
        [SerializeField] private TMP_Text _itemChangeTitleText;
        [SerializeField] private ItemWidget _itemWidgetPrefab;
        [SerializeField] private Transform _itemWidgetContainer;

        Action<UnitMode, float> onConfirmPressed;
        Action onCancelPressed;
        Action onInputModified;

        private int _selectedEntityIndex = 0;
        private ModifyMode _modifyMode;

        public enum UnitMode { DISCRETE, PERCENT };
        private UnitMode _unitMode = UnitMode.DISCRETE;

        private float InputValue
        {
            get
            {
                float inputValue = 0f;
                if (_inputField != null)
                {
                    float.TryParse(_inputField.text, out inputValue);
                }

                //No negatives!
                if (inputValue < 0f)
                {
                    inputValue = 0f;
                }

                return inputValue;
            }
        }

        public void ShowModal(ModifyMode mode, int entityIndex, Action onInput, Action<UnitMode, float> onConfirm, Action onCancelled)
        {
            SetTitle(mode.ToString());
            _modifyMode = mode;

            onInputModified = onInput;
            onConfirmPressed = onConfirm;
            onCancelPressed = onCancelled;

            if (_unitTypeContainer != null)
            {
                _unitTypeContainer.SetActive(_modifyMode == ModifyMode.HARVEST);
            }

            if (_tileSelectDropdown != null)
            {
                _tileSelectDropdown.value = 0;
                _tileSelectDropdown.RefreshShownValue();
            }

            //Always start with Discrete mode
            _unitMode = UnitMode.DISCRETE;
            if (_discreteToggle != null)
            {
                _discreteToggle.isOn = true;
            }

            if (_inputField != null)
            {
                _inputField.text = "10";
            }

            //Update entity info
            _selectedEntityIndex = entityIndex;
            SetEntityType(_selectedEntityIndex);

            //Update items
            if (_itemChangeTitleText != null)
            {
                _itemChangeTitleText.text = _modifyMode == ModifyMode.HARVEST ? "Rewards" : "Cost";
            }

            this.gameObject.SetActive(true);
            StartCoroutine(RefreshLayout());
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        #region Callbacks
        public void OnConfirmPressed()
        {
            //Send data to the Manager
            onConfirmPressed?.Invoke(_unitMode, InputValue);
        }

        public void OnCancelPressed()
        {
            HideModal();
            onCancelPressed?.Invoke();
        }

        public void OnPercentPressed()
        {
            //Failsafe -> No percentage unit mode in Introduce
            if (_modifyMode == ModifyMode.INTRODUCE)
            {
                return;
            }

            _unitMode = UnitMode.PERCENT;
            if (_inputField != null)
            {
                _inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            }

            OnInputModified();
        }

        public void OnDiscretePressed()
        {
            _unitMode = UnitMode.DISCRETE;
            if (_inputField != null)
            {
                _inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            }

            OnInputModified();
        }

        public void OnIncreasePressed()
        {
            if (_inputField != null)
            {
                _inputField.text = (InputValue + 10f).ToString();
            }

            OnInputModified();
        }

        public void OnDecreasePressed()
        {
            if (_inputField != null)
            {
                _inputField.text = (InputValue - 10f).ToString();
            }

            OnInputModified();
        }

        public void OnInputModified()
        {
            ValidateInput();
            onInputModified.Invoke();
        }

        public void ValidateInput()
        {
            //Ensure input field looks correct
            if (_unitMode == UnitMode.DISCRETE)
            {
                if (InputValue <= 0)
                {
                    if (_inputField != null)
                    {
                        _inputField.text = "0";
                    }
                }
            }
            else if (_unitMode == UnitMode.PERCENT)
            {
                float clampedInput = Mathf.Clamp(InputValue, 0f, 100f);
                if (_inputField != null)
                {
                    _inputField.text = clampedInput.ToString();
                }
            }


            //Enable/disable buttons
            if (InputValue <= 0)
            {
                if (_decreaseButton != null)
                {
                    _decreaseButton.interactable = false;
                }

                if (_increaseButton != null)
                {
                    _increaseButton.interactable = true;
                }
            }
            else
            {
                if (_decreaseButton != null)
                {
                    _decreaseButton.interactable = true;
                }

                if (_increaseButton != null)
                {
                    if ((_unitMode == UnitMode.PERCENT) && InputValue >= 100f)
                    {
                        _increaseButton.interactable = false;
                    }
                    else
                    {
                        _increaseButton.interactable = true;
                    }
                }
            }

        }
        #endregion

        #region UI
        private void SetTitle(string title)
        {
            string modifyText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToString().ToLower()); //Bit of a hack but works

            if (_title != null)
            {
                _title.text = modifyText;
            }

            if (_confirmText != null)
            {
                _confirmText.text = modifyText;
            }
        }

        private void SetEntityType(int entityIndex)
        {
            if (_selectedEntityText != null)
            {
                Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(_selectedEntityIndex);
                if (entityType != null)
                {
                    _selectedEntityText.text = string.Format($"{entityType.ID}");
                    _entityIcon?.SetEntity(entityType);

                    Quantity[] requirements = _modifyMode == ModifyMode.HARVEST ? entityType.HarvestQuantities : entityType.IntroduceQuantities;
                    InstantiateItemWidgets(requirements);
                }
            }
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            RectTransform rectTransform = this.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }

            //Check the toggles
            //These don't update until after the modal is set active,
            //So we can't rely on this to work in ShowModal()
            if (_unitMode == UnitMode.DISCRETE)
            {
                if (_discreteToggle != null)
                {
                    _discreteToggle.isOn = true;
                }
            }
            else if (_unitMode == UnitMode.PERCENT)
            {
                if (_percentToggle != null)
                {
                    _percentToggle.isOn = true;
                }
            }
        }

        public void UpdateModal(int entityIndex, List<Cell> selectedCells)
        {
            if ((SandboxManager.Instance.EntityManager == null) || (selectedCells == null))
            {
                //TODO(caspar): error
                return;
            }

            //Calculate difference
            int actualDifference = GetActualDifference(selectedCells);
            int totalPopulation = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(entityIndex);
            if (_currentPopulationText != null)
            {
                _currentPopulationText.text = totalPopulation.ToString("n0");
            }

            if (_predictedPopulationText.text != null)
            {
                _predictedPopulationText.text = (totalPopulation + actualDifference).ToString("n0");
            }

            //If we have no cells selected, automatically fail the perform check
            bool canPerformAction = true;
            if ((selectedCells.Count <= 0) || (InputValue <= 0))
            {
                canPerformAction = false;
                SetItemWidgetsVisibile(false);
            }
            //If we have cells selected, check our inventory quantities before we accept an action as valid
            else
            {
                Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(entityIndex);
                if (entityType != null)
                {
                    Quantity[] requirements = _modifyMode == ModifyMode.HARVEST ? entityType.HarvestQuantities : entityType.IntroduceQuantities;
                    if (requirements != null)
                    {
                        SetItemWidgetsVisibile(true);
                        UpdateItemWidgets(requirements, actualDifference);
                        foreach (Quantity quantity in requirements)
                        {
                            bool hasQuantity = _modifyMode == ModifyMode.INTRODUCE ? SandboxManager.Instance.PlayerInventory.HasQuantities(new Quantity[] { quantity }, Mathf.Abs(actualDifference)) : true;
                            if (!hasQuantity)
                            {
                                canPerformAction = false;
                                break;
                            }
                        }
                    }
                }
            }

            //Disable button if we don't have the requirements
            if (_confirmButton != null)
            {
                _confirmButton.interactable = canPerformAction;
            }

            //Refresh layout
            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }

        private int GetActualDifference(List<Cell> selectedCells)
        {
            if (selectedCells.Count <= 0)
            {
                return 0;
            }

            //Calculate the "actual difference"
            //For HARVEST, This will depend on the amount of available entities in each cell
            //For INTRODUCE, this is just the amount * cellcount
            int actualDifference = 0;
            if (_modifyMode == ModifyMode.HARVEST)
            {
                //Get actual change based on valid populations
                foreach (Cell cell in selectedCells)
                {
                    int cellPopulation = SandboxManager.Instance.EntityManager.GetPopulationInCell(cell.Column, cell.Row, _selectedEntityIndex);
                    int modifyAmount = _unitMode == UnitMode.DISCRETE ? Mathf.FloorToInt(InputValue) : Mathf.FloorToInt(cellPopulation * (InputValue / 100f));
                    int newPopulation = Mathf.Max(cellPopulation - modifyAmount, 0);
                    actualDifference += newPopulation - cellPopulation;
                }
            }
            else if (_modifyMode == ModifyMode.INTRODUCE)
            {
                actualDifference = Mathf.FloorToInt(InputValue) * selectedCells.Count; //Discrete only in Introduce Mode
            }

            return actualDifference;
        }

        public void SetItemWidgetsVisibile(bool value)
        {
            _itemChangeContainer?.SetActive(value);
        }

        private void InstantiateItemWidgets(Quantity[] quantities)
        {
            if ((_itemWidgetContainer == null) || (_itemWidgetPrefab == null))
            {
                return;
            }

            //Remove old objectives
            foreach (Transform child in _itemWidgetContainer)
            {
                Destroy(child.gameObject);
            }

            //Instantiate necessary number of widgets
            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                foreach (Quantity quantity in quantities)
                {
                    ItemWidget widget = Instantiate(_itemWidgetPrefab, _itemWidgetContainer);
                }
            }
        }

        private void UpdateItemWidgets(Quantity[] quantities, int modifyAmount)
        {
            if ((_itemWidgetContainer == null) || (_itemWidgetPrefab == null))
            {
                return;
            }

            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                ItemWidget[] widgets = _itemWidgetContainer.GetComponentsInChildren<ItemWidget>();
                for (int i = 0; i < quantities.Length; i++)
                {
                    if (i >= widgets.Length)
                    {
                        //TODO(caspar): We should never reach this point
                        //Spawn new widget? Just in case?
                    }
                    else
                    {
                        Item item = inventory.GetItemDef(quantities[i].ID);
                        if (item != null)
                        {
                            widgets[i].SetIcon(item.Icon);

                            int quantity = -1 * (quantities[i].Value * modifyAmount); //Invert to account for modifyAmount being the signed difference in Entity Population
                            widgets[i].SetQuantity(quantity, true);

                            bool hasQuantity = _modifyMode == ModifyMode.INTRODUCE ? SandboxManager.Instance.PlayerInventory.HasQuantities(new Quantity[] { quantities[i] }, Mathf.Abs(modifyAmount)) : true;
                            widgets[i].SetValid(hasQuantity);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
