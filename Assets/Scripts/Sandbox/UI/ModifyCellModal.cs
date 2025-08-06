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

        [Header("Text")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _selectedPopulation;
        [SerializeField] private TMP_Text _inventoryChange;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private GameObject _unitTypeContainer;

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

            //Always start with Discrete mode
            _unitMode = UnitMode.DISCRETE;
            if (_inputField != null)
            {
                _inputField.text = "10";
            }

            //Update entity info
            _selectedEntityIndex = entityIndex;
            SetEntityType(_selectedEntityIndex);
            //UpdateModal();

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

        public void OnInputModified()
        {
            ValidateInput();
            onInputModified.Invoke();
        }

        public void ValidateInput()
        {
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
        }
        #endregion

        #region UI
        private void SetTitle(string title)
        {
            if (_title != null)
            {
                _title.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()); //Bit of a hack but works
            }

            if (_confirmButton != null)
            {
                TMP_Text buttonText = _confirmButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = title;
                }
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
            if (_selectedPopulation != null)
            {
                _selectedPopulation.text = string.Format($"{totalPopulation} -> {totalPopulation + actualDifference}");
            }

            //If we have no cells selected, automatically fail the perform check
            bool canPerformAction = true;
            if (selectedCells.Count <= 0)
            {
                canPerformAction = false;
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

        /*private void UpdateInventoryPanel()
        {

        }

        private void UpdateInventoryChange(bool canPerformAction, int entityIndex)
        {
            if ((_inventoryChange == null) || (SandboxManager.Instance.EntityManager == null))
            {
                //TODO(caspar): Error
                return;
            }

            if (_selectedCells == null)
            {
                //TODO(caspar): Error
                return;
            }

            int modifyAmount = 0;
            if (_inputField != null)
            {
                int.TryParse(_inputField.text, out modifyAmount);
            }

            modifyAmount *= _selectedCells.Count;

            string inventoryList = string.Empty;
            string inventoryPrefix = string.Empty;

            //Figure out cost
            Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(entityIndex);
            if (entityType != null)
            {
                Quantity[] requirements = _actionType == PlayerAction.HARVEST ? entityType.HarvestQuantities : entityType.IntroduceQuantities;
                if (requirements != null)
                {
                    foreach (Quantity quantity in requirements)
                    {
                        bool hasRequirement = _actionType == PlayerAction.INTRODUCE ? SandboxManager.Instance.PlayerInventory.HasQuantities(new Quantity[] { quantity }, modifyAmount) : true;
                        string ownedAmount = SandboxManager.Instance.PlayerInventory.GetAmountHeld(quantity.ID).ToString();
                        string changeAmount = _actionType == PlayerAction.INTRODUCE ? string.Format($"Required {quantity.Value * modifyAmount}") : string.Format($"{quantity.Value * modifyAmount}");
                        inventoryList += string.Format($"<color={(hasRequirement ? "black" : "red")}>{quantity.ID}: {changeAmount} (Owned {ownedAmount})</color>\n");
                    }
                }
            }


            if (_selectedCells.Count <= 0)
            {
                inventoryPrefix = "<color=red\">No cells selected!</color>\n";
            }
            else
            { 
                //Sort out prefix now we know what the cost is
                if (canPerformAction)
                {
                    inventoryPrefix = _actionType == PlayerAction.HARVEST ? "You will gain:\n" : "You will spend:\n";
                }
                else
                {
                    inventoryPrefix = "You cannot afford this action:\n";
                }
            }


            _inventoryChange.text = inventoryPrefix + inventoryList;

            if (_inventoryChange.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_inventoryChange.GetComponent<RectTransform>());
            }
        }*/
        #endregion
    }
}
