using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{


    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private EntityPanel _entityPanel;
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private ObjectivePanel _objectivePanel;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ModifyCellManager _modifyCellManager;
        [SerializeField] private ResultsModal _resultsModal;

        private int _selectedEntityIndex = 0;
        public int SelectedEntityIndex => _selectedEntityIndex;

        private const string LogChannel = "[SandboxUI]";

        #region Setup
        public void Init(Entity[] entities)
        {
            //Initialise our components
            _entityPanel?.Init(entities);
            _objectivePanel?.Init();
            _inventoryPanel?.Init();
            _playerToolbar?.Init();
            _modifyCellManager?.Init();
            _resultsModal?.HideModal();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;
            _modifyCellManager.onEnterModifyMode += OnEnterModifyMode;
            _modifyCellManager.onExitModifyMode += OnExitModifyMode;
            _modifyCellManager.onModifySuccess += OnModifySuccess;

            //Force rebuild
            foreach (RectTransform child in this.GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            }
        }
        #endregion

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
