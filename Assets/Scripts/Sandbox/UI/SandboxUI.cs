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
        [SerializeField] private CurrencyPanel _currencyCounter;


        [SerializeField] private EntityPanel _entityPanel;
        [SerializeField] private ObjectivePanel _objectivePanel;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ModifyCellManager _modifyCellManager;
        [SerializeField] private ResultsModal _resultsModal;

        private int _selectedEntityIndex = 0;
        public int SelectedEntityIndex => _selectedEntityIndex;

        private const string LogChannel = "[SandboxUI]";

        #region Setup
        public void Init(EntityManager entityManager, PlayerInventory playerInventory)
        {
            //Initialise our components
            _entityPanel?.Init(entityManager.GetEntityTypeList());
            _playerToolbar?.Init();
            _modifyCellManager?.Init();
            _resultsModal?.HideModal();

            _currencyCounter?.Refresh();
            _inventoryPanel?.RefreshInventory();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;
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
            _entityPanel?.Cleanup();
            _modifyCellManager?.Cleanup();

            //TODO(caspar): We need to unsubscribe from the events as well
            //They're set to null but I don't know if I want to do that
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
            _objectivePanel?.UpdatePopulations();
            _objectivePanel?.UpdateWinConditions();
            _inventoryPanel?.RefreshInventory();
            _modifyCellManager?.ExitModifyMode();
            _playerToolbar?.UpdateActionsRemaining(actions);
        }
        #endregion

        #region Entities
        private void OnEntitySelected(int entityIndex)
        {
            //Forces reset
            if (entityIndex != _selectedEntityIndex)
            {
                _modifyCellManager?.ExitModifyMode();
            }

            //Clamp just in case?
            int maxIndex = 0;
            if (SandboxManager.Instance.EntityManager != null)
            {
                maxIndex = SandboxManager.Instance.EntityManager.EntityTypeCount;
            }

            _selectedEntityIndex = Mathf.Clamp(entityIndex, 0, maxIndex);
            _playerToolbar?.ShowToolbar(_selectedEntityIndex);
        }

        private void OnEntityUpdated(int column, int row, int id)
        {
            _objectivePanel?.OnEntityUpdated(column, row, id);
            _inventoryPanel?.RefreshInventory();
        }
        #endregion

        #region Inventory
        private void OnInventoryUpdated(string id, int amount)
        {
            _currencyCounter?.Refresh();
            _inventoryPanel?.RefreshInventory();
        }
        #endregion

        #region Modify Mode
        private void OnEnterModifyMode(ModifyMode mode)
        {
            _playerToolbar?.SetModifyModeText(mode);
        }

        private void OnModifySuccess()
        {
            int actionsRemaining = SandboxManager.GetAvailableActionPoints();
            _playerToolbar?.UpdateActionsRemaining(actionsRemaining);
        }

        private void OnExitModifyMode()
        {
            _playerToolbar?.HideToolbar();
        }
        #endregion
    }
}
