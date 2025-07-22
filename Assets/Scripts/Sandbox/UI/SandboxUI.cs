using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public enum PlayerAction { NONE, INTRODUCE, HARVEST }; //SELL ?

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
        private PlayerAction _currentAction = PlayerAction.NONE;

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

            SetCurrentAction(PlayerAction.NONE);

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

        #region Actions
        public void OnHarvestSelected()
        {
            if (SandboxManager.CanPerformAction())
            {
                SetCurrentAction(PlayerAction.HARVEST);
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
                SetCurrentAction(PlayerAction.INTRODUCE);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        private void SetCurrentAction(PlayerAction action)
        {
            _currentAction = action;

            switch (_currentAction)
            {
                case (PlayerAction.HARVEST):
                    {
                        ShowHarvestModal(_selectedEntityIndex);
                        break;
                    }

                case (PlayerAction.INTRODUCE):
                    {
                        ShowIntroduceModal(_selectedEntityIndex);
                        break;
                    }
                case (PlayerAction.NONE):
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //TODO(caspar): We won't need this later, but useful for now
            _playerToolbar?.SetActionText(_currentAction);
        }

        private void OnActionSuccess(PlayerAction actionType)
        {
            SetCurrentAction(PlayerAction.NONE);
            int actionsRemaining = SandboxManager.SpendAction();

            if (actionType == PlayerAction.HARVEST)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.HARVEST);
            }
            else if (actionType == PlayerAction.INTRODUCE)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.INTRODUCE);
            }

            //Update some UI elements
            _playerToolbar.UpdateActionsRemaining(actionsRemaining);
        }

        private void OnActionCancelled()
        {
            SetCurrentAction(PlayerAction.NONE);
            _playerToolbar?.HideToolbar();
        }
        #endregion

        #region Harvest
        private void ShowHarvestModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(PlayerAction.HARVEST, entityIndex, OnHarvestConfirmed, OnActionCancelled);
        }

        protected void OnHarvestConfirmed(int index, List<Cell> selectedCells, int amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (selectedCells.Count > 0))
            {
                foreach(Cell cell in selectedCells)
                {
                    if (SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(cell.Column, cell.Row, index, amount))
                    {
                        successCount += 1;
                    }
                }
            }

            if (successCount > 0)
            {
                OnActionSuccess(PlayerAction.HARVEST);
            }
            else
            {
                OnActionCancelled();
            }
        }
        #endregion

        #region Introduce
        private void ShowIntroduceModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(PlayerAction.INTRODUCE, entityIndex, OnIntroduceConfirmed, OnActionCancelled);
        }

        protected void OnIntroduceConfirmed(int index, List<Cell> selectedCells, int amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (selectedCells.Count > 0))
            {
                foreach (Cell cell in selectedCells)
                {
                    if (SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(cell.Column, cell.Row, index, amount))
                    {
                        successCount += 1;
                    }
                }
            }

            if (successCount > 0)
            {
                OnActionSuccess(PlayerAction.INTRODUCE);
            }
            else
            {
                OnActionCancelled();
            }
        }
        #endregion
    }
}
