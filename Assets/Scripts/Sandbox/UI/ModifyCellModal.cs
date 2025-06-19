using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Glitchers.EcoKnow.Sandbox.Grid;

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

        [Header("Entity")]
        [SerializeField] private Transform _entityButtonContainer;
        [SerializeField] private Button _entityButtonPrefab;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;

        Action<int, int> onConfirmPressed;

        private int _selectedEntity = 0;
        private PlayerAction _actionType;

        public void ShowModal(PlayerAction action, CellEntity[] entities, Action<int, int> onConfirm)
        {
            SetTitle(action.ToString());
            _actionType = action;

            if (_inputField != null)
            {
                _inputField.text = "1";
            }

            onConfirmPressed = onConfirm;
            PopulateEntityOptions(entities);

            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        #region Callbacks
        public void OnConfirmPressed()
        {
            int modifyAmount = 1;

            if (_inputField != null)
            {
                int.TryParse(_inputField.text, out modifyAmount);
            }

            //Send data to the PlayerToolbar?
            onConfirmPressed?.Invoke(_selectedEntity, modifyAmount);
        }

        public void OnCancelPressed()
        {
            HideModal();
        }

        public void OnInputModified()
        {
            UpdateInventoryChange(_selectedEntity);
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
        private void PopulateEntityOptions(CellEntity[] entities)
        {
            _selectedEntity = 0;
            
            if ((_entityButtonPrefab == null) || (_entityButtonContainer == null) || (_selectedID == null) || (_selectedPopulation == null))
            {
                return;
            }

            _selectedID.text = string.Format($"Selected: {entities[_selectedEntity].ID}");
            _selectedPopulation.text = string.Format($"Current Population: {entities[_selectedEntity].Population}");
            UpdateInventoryChange(_selectedEntity);

            foreach (Transform child in _entityButtonContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < entities.Length; i++)
            {
                Button button = Instantiate(_entityButtonPrefab, _entityButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = entities[i].ID;
                }

                int entityIndex = i;
                button.onClick.AddListener( delegate {
                    _selectedEntity = entityIndex;
                    _selectedID.text = string.Format($"Selected: {entities[entityIndex].ID}");
                    _selectedPopulation.text = string.Format($"Current Population: {entities[entityIndex].Population}");
                    UpdateInventoryChange(entityIndex);
                } );
            }

            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }

        private void UpdateInventoryChange(int entityIndex)
        {
            if ((_inventoryChange == null) || (SandboxManager.Instance.EntityManager == null))
            {
                return;
            }

            int modifyAmount = 0;
            if (_inputField != null)
            {
                int.TryParse(_inputField.text, out modifyAmount);
            }

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

            //Sort out prefix now we know what the cost is
            if (canPerformAction)
            {
                inventoryPrefix = _actionType == PlayerAction.HARVEST ? "You will gain:\n" : "You will spend:\n";
            }
            else
            {
                inventoryPrefix = "You cannot afford this action:\n";
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
