using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SandboxUI : MonoBehaviour
    {
        [Header("Player Side Panel")]
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private CurrencyCounter _currencyCounter;
        [SerializeField] private CurrencyCounter _actionPointCounter;

        [Header("Entity and Objective Panels")]
        [SerializeField] private EntityPanel _entityPanel;
        [SerializeField] private ObjectivesPanel _objectivePanel;
        //[SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ToolPanel _toolPanel;

        [Header("Modification Panels")]
        [SerializeField] private ModifyCellManager _modifyCellManager;

        [Header("Inventory")]
        [SerializeField] private InventoryPanel _inventoryPanel;

        [Header("Results")]
        [SerializeField] private PopulationGraph _populationGraph;
        [SerializeField] private ResultsModal _resultsModal;

        public int SelectedEntityIndex => _entityPanel == null ? -1 : _entityPanel.SelectedEntityIndex;

        private const string LogChannel = "[SandboxUI]";

        #region Setup
        public void Init(EntityManager entityManager, WinCondition[] winConditions, PlayerInventory playerInventory)
        {
            //Initialise our components
            _entityPanel?.Init(entityManager.GetEntityTypeList());
            _objectivePanel?.Init(winConditions, entityManager);
            _toolPanel?.Init();
            _modifyCellManager?.Init();
            _populationGraph?.Init();
            _resultsModal?.HideModal();

            RefreshInventories();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;
            _entityPanel.onEntityDeselected += OnEntityDeselected;
            _modifyCellManager.onEnterModifyMode += OnEnterModifyMode;
            _modifyCellManager.onExitModifyMode += OnExitModifyMode;
            _modifyCellManager.onModifySuccess += OnModifySuccess;

            //Subscribe to other events
            if (entityManager != null)
            {
                entityManager.onEntityHarvested += OnEntityUpdated;
                entityManager.onEntityIntroduced += OnEntityUpdated;
            }
            if (playerInventory != null)
            {
                playerInventory.onItemSold += OnInventoryUpdated;
            }

            //Force rebuild
            foreach (RectTransform child in this.GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            }
        }

        public void Cleanup()
        {
            //Unsubscribe
            _entityPanel.onEntitySelected -= OnEntitySelected;
            _entityPanel.onEntityDeselected -= OnEntityDeselected;
            _modifyCellManager.onEnterModifyMode -= OnEnterModifyMode;
            _modifyCellManager.onExitModifyMode -= OnExitModifyMode;
            _modifyCellManager.onModifySuccess -= OnModifySuccess;

            _entityPanel?.Cleanup();
            _modifyCellManager?.Cleanup();
        }
        #endregion

        #region Game Lifecycle
        public void OnGameEnded(SandboxManager.Result result)
        {
            _resultsModal?.ShowModal(result.ToString());
        }

        public void OnNewRoundStarted(int currentRound, int maxRounds, int actions)
        {
            _roundIndicator?.UpdateRoundCounter(currentRound, maxRounds);

            _objectivePanel?.UpdateWinConditions(currentRound, maxRounds);

            _entityPanel?.DeselectEntity();
            _entityPanel?.UpdateAllWidgets();

            _modifyCellManager?.ExitModifyMode();

            //_playerToolbar?.UpdateActionsRemaining(actions);

            RefreshInventories();
        }
        #endregion

        #region Entities
        private void OnEntitySelected(int entityIndex)
        {
            //Forces reset
            if (_modifyCellManager.IsModifying)
            {
                _modifyCellManager?.ExitModifyMode();
            }

            _toolPanel?.ShowToolbar(entityIndex, _entityPanel.GetWidgetForEntity(entityIndex));
        }

        private void OnEntityDeselected()
        {
            if (_modifyCellManager.IsModifying)
            {
                _modifyCellManager?.ExitModifyMode();
            }

            _toolPanel?.HideToolbar();
        }

        private void OnEntityUpdated(int column, int row, int id)
        {
            _objectivePanel?.OnEntityUpdated(column, row, id);
            _entityPanel?.OnEntityUpdated(column, row, id);
        }
        #endregion

        #region Inventory
        private void OnInventoryUpdated(string id, int amount)
        {
            RefreshInventories();
        }

        private void RefreshInventories()
        {
            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                _currencyCounter.SetCurrencyText(inventory.GetAmountHeld(PlayerInventory.CurrencyID));
                _actionPointCounter?.SetCurrencyText(inventory.GetAmountHeld(PlayerInventory.ActionID));
            }

            _inventoryPanel?.RefreshInventory();
        }
        #endregion

        #region Modify Mode
        private void OnEnterModifyMode(ModifyMode mode)
        {
            //_playerToolbar?.SetModifyModeText(mode);
        }

        private void OnModifySuccess()
        {
            int actionsRemaining = SandboxManager.GetAvailableActionPoints();
            //_playerToolbar?.UpdateActionsRemaining(actionsRemaining);

            RefreshInventories();

            _entityPanel.DeselectEntity();
        }

        private void OnExitModifyMode()
        {
            //_playerToolbar?.HideToolbar();
        }
        #endregion
    }
}
