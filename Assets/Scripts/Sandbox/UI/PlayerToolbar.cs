using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public enum PlayerAction { NONE, INTRODUCE, HARVEST }; //SELL ?

    public class PlayerToolbar : MonoBehaviour
    {
        private PlayerAction _currentAction = PlayerAction.NONE;
        public PlayerAction CurrentAction => _currentAction;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;
        [SerializeField] private TMP_Text _actionsRemainingText;
        [SerializeField] private ModifyCellModal _modifyCellModal;

        [Header("Buttons")]
        [SerializeField] private Button _harvestButton;
        [SerializeField] private Button _introduceButton;

        private int _selectedEntityIndex = 0;

        private const string LogChannel = "[Toolbar]";

        public void Init()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.OnEntityHarvested += OnEntityHarvested;
                SandboxManager.Instance.EntityManager.OnEntityIntroduced += OnEntityIntroduced;
            }

            SetCurrentAction(PlayerAction.NONE);

            _modifyCellModal?.HideModal();
            HideToolbar();
        }


        public void ShowToolbar(int entityIndex)
        {
            if (entityIndex != _selectedEntityIndex)
            {
                _modifyCellModal?.HideModal();
            }

            _selectedEntityIndex = entityIndex;
            this.gameObject.SetActive(true);
        }

        public void HideToolbar()
        {
            this.gameObject.SetActive(false);
        }

        public void OnEntityHarvested(int column, int row, int id)
        {
            SetCurrentAction(PlayerAction.NONE);
            OnActionSuccess();

            Data.DataManager.Instance.RecordEvent(Data.EventType.HARVEST);
        }

        public void OnEntityIntroduced(int column, int row, int id)
        {
            SetCurrentAction(PlayerAction.NONE);
            OnActionSuccess();

            Data.DataManager.Instance.RecordEvent(Data.EventType.INTRODUCE);
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

        private void OnActionCancelled()
        {
            SetCurrentAction(PlayerAction.NONE);
            HideToolbar();
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
        private void ShowHarvestModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(PlayerAction.HARVEST, entityIndex, OnHarvestConfirmed, OnActionCancelled);
        }

        protected void OnHarvestConfirmed(int index, int amount)
        {
            _modifyCellModal?.HideModal();


            /*if ((SandboxManager.Instance.EntityManager != null) && (_selectedCells.Count > 0))
            {
                foreach(Cell cell in _selectedCells)
                {
                    //TODO(caspar): We might want to change this so that we return CanHarvest before harvesting all of the cells?
                    bool success = false;
                    success = SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(cell.Column, cell.Row, index, amount);
                }
            }*/



            /*if ((_selectedCell != null) && (SandboxManager.Instance.EntityManager != null))
            {
                //TODO(caspar): Rethink this -> Should we respond to an event rather than returning success?
                bool success = false;
                success = SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(_selectedCell.Column, _selectedCell.Row, index, amount);

                //if (success)
                //{
                //    SetCurrentAction(PlayerAction.NONE);
                //   OnActionSuccess();
                //
                //    Data.DataManager.Instance.RecordEvent(Data.EventType.HARVEST);
                //}
            }*/
        }
        #endregion

        #region Introduce
        private void ShowIntroduceModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(PlayerAction.INTRODUCE, entityIndex, OnIntroduceConfirmed, OnActionCancelled);
        }

        protected void OnIntroduceConfirmed(int index, int amount)
        {
            _modifyCellModal?.HideModal();

            /*if ((SandboxManager.Instance.EntityManager != null) && (_selectedCells.Count > 0))
            {
                foreach (Cell cell in _selectedCells)
                {
                    //TODO(caspar): We might want to change this so that we return CanIntroduce before introducing to all of the cells?
                    bool success = false;
                    success = SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(cell.Column, cell.Row, index, amount);
                }
            }*/


            /*if ((_selectedCell != null) && (SandboxManager.Instance.EntityManager != null))
            {
                bool success = false;
                success = SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(_selectedCell.Column, _selectedCell.Row, index, amount);

                //if (success)
                //{
                //    SetCurrentAction(PlayerAction.NONE);
                //    OnActionSuccess();
                //
                //    Data.DataManager.Instance.RecordEvent(Data.EventType.INTRODUCE);
                //}
            }*/
        }
        #endregion
    }
}
