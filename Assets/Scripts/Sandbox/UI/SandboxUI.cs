using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    //TODO(caspar): Maybe "Modify Mode" needs to live in a Modify handler script rather than at the top level of the SandboxUI
    public enum ModifyMode { NONE, INTRODUCE, HARVEST };

    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private EntityPanel _entityPanel;
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private ObjectivePanel _objectivePanel;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ModifyCellModal _modifyCellModal;
        [SerializeField] private ResultsModal _resultsModal;

        private int _selectedEntityIndex = 0;
        private ModifyMode _currentMode = ModifyMode.NONE;

        private const string LogChannel = "[SandboxUI]";

        #region Setup
        public void Init(Entity[] entities)
        {
            //Initialise our components
            _entityPanel?.Init(entities);
            _objectivePanel?.Init();
            _inventoryPanel?.Init();
            _playerToolbar?.Init();
            _modifyCellModal?.HideModal();
            _resultsModal?.HideModal();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;

            SetCurrentMode(ModifyMode.NONE);

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
                _modifyCellModal?.HideModal();
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
            _playerToolbar?.UpdateActionsRemaining(actions);
        }
        #endregion

        #region Harvest and Introduce
        public void OnHarvestSelected()
        {
            if (SandboxManager.CanPerformAction())
            {
                SetCurrentMode(ModifyMode.HARVEST);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        public void OnIntroduceSelected()
        {
            if (SandboxManager.CanPerformAction())
            {
                SetCurrentMode(ModifyMode.INTRODUCE);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        private void SetCurrentMode(ModifyMode mode)
        {
            _currentMode = mode;

            switch (_currentMode)
            {
                case (ModifyMode.HARVEST):
                    {
                        ShowHarvestModal(_selectedEntityIndex);
                        break;
                    }

                case (ModifyMode.INTRODUCE):
                    {
                        ShowIntroduceModal(_selectedEntityIndex);
                        break;
                    }
                case (ModifyMode.NONE):
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //TODO(caspar): We won't need this later, but useful for now
            _playerToolbar?.SetModifyModeText(_currentMode);
        }

        private void OnModifySuccess(ModifyMode modifyMode)
        {
            SetCurrentMode(ModifyMode.NONE);
            int actionsRemaining = SandboxManager.SpendActionPoint();

            if (modifyMode == ModifyMode.HARVEST)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.HARVEST);
            }
            else if (modifyMode == ModifyMode.INTRODUCE)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.INTRODUCE);
            }

            //Update some UI elements
            _playerToolbar?.UpdateActionsRemaining(actionsRemaining);
        }

        private void OnModifyCancelled()
        {
            SetCurrentMode(ModifyMode.NONE);
            _playerToolbar?.HideToolbar();
        }
        #endregion

        #region Harvest
        private void ShowHarvestModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(ModifyMode.HARVEST, entityIndex, OnHarvestConfirmed, OnModifyCancelled);
        }

        protected void OnHarvestConfirmed(int index, List<Cell> selectedCells, ModifyCellModal.UnitMode unitMode, float amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (selectedCells.Count > 0))
            {
                foreach(Cell cell in selectedCells)
                {
                    int currentPopulation = cell.GetCellEntities()[index].Population;
                    int actualAmount = unitMode == ModifyCellModal.UnitMode.DISCRETE ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(currentPopulation * (amount / 100f));
                    if (SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(cell.Column, cell.Row, index, actualAmount))
                    {
                        successCount += 1;
                    }
                }
            }

            if (successCount > 0)
            {
                OnModifySuccess(ModifyMode.HARVEST);
            }
            else
            {
                OnModifyCancelled();
            }
        }
        #endregion

        #region Introduce
        private void ShowIntroduceModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(ModifyMode.INTRODUCE, entityIndex, OnIntroduceConfirmed, OnModifyCancelled);
        }

        protected void OnIntroduceConfirmed(int index, List<Cell> selectedCells, ModifyCellModal.UnitMode unitMode, float amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (selectedCells.Count > 0))
            {
                foreach (Cell cell in selectedCells)
                {
                    int actualAmount = Mathf.FloorToInt(amount);
                    if (SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(cell.Column, cell.Row, index, actualAmount))
                    {
                        successCount += 1;
                    }
                }
            }

            if (successCount > 0)
            {
                OnModifySuccess(ModifyMode.INTRODUCE);
            }
            else
            {
                OnModifyCancelled();
            }
        }
        #endregion
    }
}
