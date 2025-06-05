using Glitchers.EcoKnow.Sandbox.Grid;
using TMPro;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PlayerToolbar : MonoBehaviour
    {
        public enum PlayerAction { NONE, INTRODUCE, HARVEST };
        private PlayerAction _currentAction = PlayerAction.NONE;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;
        [SerializeField] private ModifyCellModal _modifyCellModal;

        private Cell _selectedCell = null;

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
            SetCurrentAction(PlayerAction.HARVEST);
        }

        public void OnIntroducePressed()
        {
            SetCurrentAction(PlayerAction.INTRODUCE);
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
        #endregion

        #region Harvest
        private void ShowHarvestModal(CellEntity[] cellEntities)
        {
            _modifyCellModal?.ShowModal("Harvest", cellEntities, OnHarvestConfirmed);
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
                }
            }
        }
        #endregion

        #region Introduce
        private void ShowIntroduceModal(CellEntity[] cellEntities)
        {
            _modifyCellModal?.ShowModal("Introduce", cellEntities, OnIntroduceConfirmed);
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
                }
            }
        }
        #endregion
    }
}
