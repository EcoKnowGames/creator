using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PlayerToolbar : MonoBehaviour
    {
        public enum Action { NONE, INTRODUCE, HARVEST };
        private Action _currentAction = Action.NONE;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;
        [SerializeField] private HarvestModal _harvestModal;

        private Cell _selectedCell = null;

        public void Init()
        {
            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.gridEvents.OnCellClicked += OnCellClicked;
            }

            SetCurrentAction(Action.NONE);

            _harvestModal?.HideModal();
        }


        public void OnCellClicked(Cell cell)
        {
            if (_currentAction == Action.HARVEST)
            {
                _selectedCell = cell;
                ShowHarvestModal(_selectedCell.GetCellEntities());
                return;
            }

            //TODO(caspar) -> Do we need to sort this out???
            //TODO(caspar) -> Sort this out? We probably want to check that the cell is valid before we try to do anything to it
            //So within bounds, etc

            bool success = false;

            EntityManager entityManager = SandboxManager.Instance.EntityManager;
            if (entityManager != null)
            {
                if (_currentAction == Action.HARVEST)
                {
                    success = SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(cell.Column, cell.Row, 0, 50);
                }
                else if (_currentAction == Action.INTRODUCE)
                {
                    success = SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(cell.Column, cell.Row, 0, 20);
                }
            }

            if (success)
            {
                SetCurrentAction(Action.NONE);
            }
        }

        public void OnHarvestPressed()
        {
            SetCurrentAction(Action.HARVEST);
        }

        public void OnIntroducePressed()
        {
            SetCurrentAction(Action.INTRODUCE);
        }

        #region Actions
        private void SetCurrentAction(Action action)
        {
            _currentAction = action;

            switch (_currentAction)
            {
                case (Action.NONE):
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
        #endregion

        #region Harvest
        private void ShowHarvestModal(CellEntity[] cellEntities)
        {
            _harvestModal?.ShowModal(cellEntities, OnHarvestConfirmed);
        }

        protected void OnHarvestConfirmed(int index, int amount)
        {
            _harvestModal?.HideModal();

            if (_selectedCell != null)
            {
                bool success = false;
                success = SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(_selectedCell.Column, _selectedCell.Row, index, amount);

                if (success)
                {
                    SetCurrentAction(Action.NONE);
                }
            }
        }
        #endregion
    }
}
