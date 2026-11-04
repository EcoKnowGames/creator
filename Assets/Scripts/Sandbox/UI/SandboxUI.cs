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
        [SerializeField] private ZonePanel _zonePanel;

        [Header("Entity and Objective Panels")]
        [SerializeField] private EntityPanel _entityPanel;
        [SerializeField] private ObjectivesPanel _objectivePanel;
        [SerializeField] private ObjectivesModal _objectivesModal;
        [SerializeField] private ToolPanel _toolPanel;

        [Header("Modification Panels")]
        [SerializeField] private ModifyCellManager _modifyCellManager;

        [Header("Inventory")]
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private InventoryModal _inventoryModal;

        [Header("Results")]
        [SerializeField] private PopulationGraph _populationGraph;
        [SerializeField] private SummaryModal _summaryModal;

        [Header("Multiplayer")]
        [SerializeField] private MultiplayerBorder _multiplayerBorder;
        [SerializeField] private MultiplayerModal _multiplayerModal;

        public int SelectedEntityIndex => _entityPanel == null ? -1 : _entityPanel.SelectedEntityIndex;

        //Stored so Cleanup unsubscribes from the same instances Init subscribed to
        private EntityManager _entityManager;
        private PlayerInventory _playerInventory;

        private const string LogChannel = "[SandboxUI]";

        #region Setup
        public void Init(Scenario scenario, EntityManager entityManager, WinCondition[] winConditions, PlayerInventory playerInventory)
        {
            //Initialise our components
            _objectivesModal?.Init(scenario.Name, scenario.Author, scenario.Description, scenario.CoverImageBase64);
            _entityPanel?.Init(entityManager.GetEntityTypeList());
            _objectivePanel?.Init(winConditions, entityManager);
            _toolPanel?.Init();
            _modifyCellManager?.Init();
            _populationGraph?.Init();
            _summaryModal?.HideModal();

            if (scenario.Map.gridDef.HasZones())
            {
                _zonePanel?.SetZones(scenario.Map.gridDef.zoneDefs);
            }
            else
            {
                _zonePanel?.gameObject?.SetActive(false);
            }

            RefreshInventories();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;
            _entityPanel.onEntityDeselected += OnEntityDeselected;
            _modifyCellManager.onEnterModifyMode += OnEnterModifyMode;
            _modifyCellManager.onExitModifyMode += OnExitModifyMode;
            _modifyCellManager.onModifySuccess += OnModifySuccess;
            _inventoryModal.onSellSuccess += OnSellSuccess;

            //Subscribe to other events
            _entityManager = entityManager;
            _playerInventory = playerInventory;
            if (_entityManager != null)
            {
                _entityManager.onEntityHarvested += OnEntityUpdated;
                _entityManager.onEntityIntroduced += OnEntityUpdated;
            }
            if (_playerInventory != null)
            {
                _playerInventory.onItemSold += OnInventoryUpdated;
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
            if (_inventoryModal != null)
            {
                _inventoryModal.onSellSuccess -= OnSellSuccess;
            }

            if (_entityManager != null)
            {
                _entityManager.onEntityHarvested -= OnEntityUpdated;
                _entityManager.onEntityIntroduced -= OnEntityUpdated;
            }
            if (_playerInventory != null)
            {
                _playerInventory.onItemSold -= OnInventoryUpdated;
            }
            _entityManager = null;
            _playerInventory = null;

            _entityPanel?.Cleanup();
            _modifyCellManager?.Cleanup();
        }
        #endregion

        #region Game Lifecycle
        public bool IsFocused()
        {
            if (_objectivesModal.isActiveAndEnabled)
            {
                return true;
            }

            if (_summaryModal.isActiveAndEnabled)
            {
                return true;
            }

            if (_inventoryModal.isActiveAndEnabled)
            {
                return true;
            }

            if (_populationGraph.IsVisible)
            {
                return true;
            }

            return false;
        }

        public void OnGameEnded(SandboxManager.Result result)
        {
            _summaryModal?.ShowModal();
        }

        public void OnNewRoundStarted(int currentRound, int maxRounds, int actions)
        {
            _roundIndicator?.UpdateRoundCounter(currentRound, maxRounds);

            _objectivePanel?.UpdateWinConditions(currentRound, maxRounds);

            _entityPanel?.DeselectEntity();
            _entityPanel?.UpdateAllWidgets();

            _modifyCellManager?.ExitModifyMode();
            _modifyCellManager?.ResetLimits();

            _inventoryModal?.HideModal();

            RefreshInventories();
        }

        public void OnActionCompleted()
        {
            _modifyCellManager?.ResetLimits();
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
            _objectivePanel?.OnInventoryUpdated(id, amount);
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

        private void OnSellSuccess()
        {
            SandboxManager.CompleteAction(Data.EventType.SELL);
            RefreshInventories();
        }
        #endregion

        #region Modify Mode
        private void OnEnterModifyMode(ModifyMode mode)
        {

        }

        private void OnModifySuccess()
        {
            RefreshInventories();
            _entityPanel.DeselectEntity();
        }

        private void OnExitModifyMode()
        {

        }
        #endregion

        #region Multiplayer
        public void UpdateMultiplayer(int currentPlayer, bool isNewRound = false, bool isMultiplayer = false)
        {
            _multiplayerBorder.SetBorderVisible(isMultiplayer);

            if (MultiplayerManager.Instance != null)
            {
                Color playerColour = MultiplayerManager.Instance.GetPlayerColour(currentPlayer);
                _multiplayerBorder.SetBorderColour(playerColour);

                string playerName = MultiplayerManager.Instance.GetPlayerName(currentPlayer);
                _multiplayerBorder?.SetPlayer(playerName);
            

                if (isMultiplayer)
                {
                    _multiplayerModal?.ShowModal(playerName, isNewRound);
                }
                else
                {
                    _multiplayerModal.HideModal();
                }
            }
        }
        #endregion
    }
}
