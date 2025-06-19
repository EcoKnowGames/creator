using System;
using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public enum PlayerAction { NONE, INTRODUCE, HARVEST };

    public class PlayerToolbar : MonoBehaviour
    {
        private PlayerAction _currentAction = PlayerAction.NONE;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;
        [SerializeField] private TMP_Text _actionsRemainingText;
        [SerializeField] private ModifyCellModal _modifyCellModal;

        [Header("Buttons")]
        [SerializeField] private Button _harvestButton;
        [SerializeField] private Button _introduceButton;

        private Cell _selectedCell = null;

        private const string LogChannel = "[Toolbar]";

        public void Init()
        {
            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.gridEvents.OnCellClicked += OnCellClicked;
            }

            SetCurrentAction(PlayerAction.NONE);

            _modifyCellModal?.HideModal();
        }


        public void OnCellClicked(Cell cell)
        {
            if (_currentAction == PlayerAction.HARVEST)
            {
                _selectedCell = cell;
                ShowHarvestModal(_selectedCell.GetCellEntities());
            }
            else if (_currentAction == PlayerAction.INTRODUCE)
            {
                _selectedCell = cell;
                ShowIntroduceModal(_selectedCell.GetCellEntities());
            }
        }

        public void OnHarvestPressed()
        {
            if (CanPerformAction())
            {
                SetCurrentAction(PlayerAction.HARVEST);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        public void OnIntroducePressed()
        {
            if (CanPerformAction())
            { 
                SetCurrentAction(PlayerAction.INTRODUCE);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        #region Actions
        private void SetCurrentAction(PlayerAction action)
        {
            _currentAction = action;

            switch (_currentAction)
            {
                case (PlayerAction.NONE):
                    {
                        _selectedCell = null;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


            if (_activeModeText != null)
            {
                _activeModeText.text = _currentAction.ToString();
            }
        }

        private bool CanPerformAction()
        {
            PlayerInventory inventory = SandboxManager.Instance.PlayerInventory;
            if (inventory != null)
            {
                return inventory.GetAmountHeld(PlayerInventory.ActionID) > 0;
            }

            return false;
        }

        private void OnActionSuccess()
        {
            int actionsRemaining = 0;

            //TODO(caspar): OnActionSuccess?
            //TODO(caspar): When do we send the data to the command/event recording layer?
            if (SandboxManager.Instance.PlayerInventory != null)
            {
                actionsRemaining = SandboxManager.Instance.PlayerInventory.RemoveItem(PlayerInventory.ActionID, 1);
            }

            UpdateActionsRemaining(actionsRemaining);
        }

        public void UpdateActionsRemaining(int actions)
        {
            if (_actionsRemainingText != null)
            {
                _actionsRemainingText.text = string.Format($"Actions Left: {actions}");
            }


            //Set out buttons active or not
            bool hasActions = actions > 0;
            if (_harvestButton != null)
            {
                _harvestButton.interactable = hasActions;
            }

            if (_introduceButton != null)
            {
                _introduceButton.interactable = hasActions;
            }
        }
        #endregion

        #region Harvest
        private void ShowHarvestModal(CellEntity[] cellEntities)
        {
            _modifyCellModal?.ShowModal(PlayerAction.HARVEST, cellEntities, OnHarvestConfirmed);
        }

        protected void OnHarvestConfirmed(int index, int amount)
        {
            _modifyCellModal?.HideModal();

            if ((_selectedCell != null) && (SandboxManager.Instance.EntityManager != null))
            {
                bool success = false;
                success = SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(_selectedCell.Column, _selectedCell.Row, index, amount);

                if (success)
                {
                    SetCurrentAction(PlayerAction.NONE);
                    OnActionSuccess();
                }
            }
        }
        #endregion

        #region Introduce
        private void ShowIntroduceModal(CellEntity[] cellEntities)
        {
            _modifyCellModal?.ShowModal(PlayerAction.INTRODUCE, cellEntities, OnIntroduceConfirmed);
        }

        protected void OnIntroduceConfirmed(int index, int amount)
        {
            _modifyCellModal?.HideModal();

            if ((_selectedCell != null) && (SandboxManager.Instance.EntityManager != null))
            {
                bool success = false;
                success = SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(_selectedCell.Column, _selectedCell.Row, index, amount);

                if (success)
                {
                    SetCurrentAction(PlayerAction.NONE);
                    OnActionSuccess();
                }
            }
        }
        #endregion
    }
}
