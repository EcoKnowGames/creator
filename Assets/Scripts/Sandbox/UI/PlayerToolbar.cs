using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class PlayerToolbar : MonoBehaviour
    {
        public enum Action { NONE, INTRODUCE, HARVEST };
        private Action _currentAction = Action.NONE;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _activeModeText;

        public void Init()
        {
            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.gridEvents.OnCellClicked += OnCellClicked;
            }

            SetCurrentAction(Action.NONE);
        }


        public void OnCellClicked(Cell cell)
        {
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

            if (_activeModeText != null)
            {
                _activeModeText.text = _currentAction.ToString();
            }
        }
        #endregion
    }
}
