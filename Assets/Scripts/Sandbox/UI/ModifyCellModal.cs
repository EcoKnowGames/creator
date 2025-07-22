using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ModifyCellModal : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _selectedID;
        [SerializeField] private TMP_Text _selectedPopulation;
        [SerializeField] private TMP_Text _inventoryChange;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;

        Action<int, int> onConfirmPressed;
        Action onCancelPressed;

        private int _selectedEntity = 0;
        private List<Cell> _selectedCells = new List<Cell>();
        private PlayerAction _actionType;

        public void ShowModal(PlayerAction action, int entityIndex, Action<int, int> onConfirm, Action onCancelled)
        {
            SetTitle(action.ToString());
            _actionType = action;

            if (_inputField != null)
            {
                _inputField.text = "1";
            }

            onConfirmPressed = onConfirm;
            onCancelPressed = onCancelled;

            //Just in case we re-open the modal in a different mode?
            //TODO(caspar): This will probably have a better solution once the real UI is in place
            UnsubscribeFromEvents();
            SubscribeToEvents();

            //TODO(caspar): We are repeating ourselves -> maybe we just need to run Hide first?
            ClearSelectedCells();

            _selectedEntity = entityIndex;
            RefreshEntityOptions();

            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);

            ClearSelectedCells();
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.gridEvents.OnCellClicked += OnCellClicked;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.gridEvents.OnCellClicked -= OnCellClicked;
            }
        }


        #region Callbacks
        public void OnCellClicked(Cell cell)
        {
            if (_selectedCells.Contains(cell))
            {
                _selectedCells.Remove(cell);
                cell.ShowSelected(false);
            }
            else
            {
                _selectedCells.Add(cell);
                cell.ShowSelected(true);
            }

            RefreshEntityOptions();
        }


        public void OnConfirmPressed()
        {
            int modifyAmount = 1;
            if (_inputField != null)
            {
                int.TryParse(_inputField.text, out modifyAmount);
            }

            //Send data to the PlayerToolbar
            onConfirmPressed?.Invoke(_selectedEntity, modifyAmount);
        }

        public void OnCancelPressed()
        {
            HideModal();
            onCancelPressed?.Invoke();
        }

        public void OnInputModified()
        {
            RefreshEntityOptions();
        }
        #endregion

        #region Cells and Entities
        private void ClearSelectedCells()
        {
            foreach (Cell cell in _selectedCells)
            {
                cell.ShowSelected(false);
            }

            _selectedCells.Clear();
        }
        #endregion

        #region UI
        private void SetTitle(string title)
        {
            if (_title != null)
            {
                _title.text = title;
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
        private void RefreshEntityOptions()
        {
            if ((_selectedID == null) || (_selectedPopulation == null) || (SandboxManager.Instance.EntityManager == null))
            {
                return;
            }

            Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(_selectedEntity);
            if (entityType != null)
            {
                int totalPopulation = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(_selectedEntity);

                _selectedID.text = string.Format($"Selected: {entityType.ID}");
                _selectedPopulation.text = string.Format($"Current Population: {totalPopulation}");
                UpdateInventoryChange(_selectedEntity);
            }

            //Refresh layout
            //TODO(caspar): Do we still need to do this now that we don't have buttons?
            //Might be useful for the new UI anyway
            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }

        private void UpdateInventoryChange(int entityIndex)
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
            bool canPerformAction = true;
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

                        if (!hasRequirement)
                        {
                            canPerformAction = false;
                        }
                    }
                }
            }


            if (modifyAmount <= 0)
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
        }
        #endregion
    }
}
