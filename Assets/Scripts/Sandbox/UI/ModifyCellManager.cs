using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public enum ModifyMode { NONE, INTRODUCE, HARVEST };

    public class ModifyCellManager : MonoBehaviour
    {
        [SerializeField] private ModifyCellModal _modifyCellModal;
        private SandboxUI _sandboxUI => this.GetComponentInParent<SandboxUI>();
        private int _selectedEntityIndex => _sandboxUI != null ? _sandboxUI.SelectedEntityIndex : 0; //The parent UI should be the authority here



        private ModifyMode _modifyMode = ModifyMode.NONE;
        public ModifyMode CurrentMode => _modifyMode;
        public bool IsModifying => _modifyMode != ModifyMode.NONE;


        public Action<ModifyMode> onEnterModifyMode;
        public Action onModifySuccess;
        public Action onExitModifyMode;

        private List<Cell> _selectedCells = new List<Cell>();

        private const string LogChannel = "[ModifyCellManager]";

        public void Init()
        {
            ExitModifyMode();

            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.OnCellClicked += OnCellClicked;
            }
        }

        public void Cleanup()
        {
            ExitModifyMode();

            onEnterModifyMode = null;
            onModifySuccess = null;
            onExitModifyMode = null;

            if (SandboxManager.Instance.GridManager != null)
            {
                SandboxManager.Instance.GridManager.OnCellClicked -= OnCellClicked;
            }
        }

        #region Callbacks
        public void OnHarvestSelected()
        {
            if (SandboxManager.CanPerformAction())
            {
                EnterModifyMode(ModifyMode.HARVEST);
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
                EnterModifyMode(ModifyMode.INTRODUCE);
            }
            else
            {
                Debug.LogError($"{LogChannel} Cannot select action, no action points remaining!");
            }
        }

        public void OnInputModified()
        {
            _modifyCellModal?.UpdateModal(_selectedEntityIndex, _selectedCells);
        }

        public void OnCellClicked(Cell cell)
        {
            if (IsModifying)
            {
                if (_selectedCells.Contains(cell))
                {
                    _selectedCells.Remove(cell);
                    cell.ShowSelected(false);
                }
                else
                {
                    _selectedCells.Add(cell);
                    cell.ShowSelected(true);
                }

                _modifyCellModal?.UpdateModal(_selectedEntityIndex, _selectedCells);
            }
        }

        private void ClearSelectedCells()
        {
            foreach (Cell cell in _selectedCells)
            {
                cell.ShowSelected(false);
            }

            _selectedCells.Clear();
        }
        #endregion

        private void EnterModifyMode(ModifyMode mode)
        {
            _modifyMode = mode;
            ClearSelectedCells();

            switch (_modifyMode)
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

            onEnterModifyMode?.Invoke(_modifyMode);
        }

        public void ExitModifyMode()
        {
            _modifyMode = ModifyMode.NONE;
            _modifyCellModal?.HideModal();
            ClearSelectedCells();

            onExitModifyMode?.Invoke();
        }

        #region Events
        private void OnModifySuccess(ModifyMode modifyMode)
        {
            ExitModifyMode();
            int actionsRemaining = SandboxManager.SpendActionPoint();

            if (modifyMode == ModifyMode.HARVEST)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.HARVEST);
            }
            else if (modifyMode == ModifyMode.INTRODUCE)
            {
                Data.DataManager.Instance.RecordEvent(Data.EventType.INTRODUCE);
            }

            onModifySuccess?.Invoke();
        }

        private void OnModifyCancelled()
        {
            ExitModifyMode();
        }
        #endregion

        #region Harvest
        private void ShowHarvestModal(int entityIndex)
        {
            _modifyCellModal?.ShowModal(ModifyMode.HARVEST, entityIndex, OnInputModified, OnHarvestConfirmed, OnModifyCancelled);
        }

        protected void OnHarvestConfirmed(ModifyCellModal.UnitMode unitMode, float amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (_selectedCells.Count > 0))
            {
                foreach (Cell cell in _selectedCells)
                {
                    int cellPopulation = SandboxManager.Instance.EntityManager.GetPopulationInCell(cell.Column, cell.Row, _selectedEntityIndex);
                    int actualAmount = unitMode == ModifyCellModal.UnitMode.DISCRETE ? Mathf.FloorToInt(amount) : Mathf.FloorToInt(cellPopulation * (amount / 100f));
                    if (SandboxManager.Instance.EntityManager.TryHarvestEntityFromCell(cell.Column, cell.Row, _selectedEntityIndex, actualAmount))
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
            _modifyCellModal?.ShowModal(ModifyMode.INTRODUCE, entityIndex, OnInputModified, OnIntroduceConfirmed, OnModifyCancelled);
        }

        protected void OnIntroduceConfirmed(ModifyCellModal.UnitMode unitMode, float amount)
        {
            _modifyCellModal?.HideModal();

            int successCount = 0;
            if ((SandboxManager.Instance.EntityManager != null) && (_selectedCells.Count > 0))
            {
                foreach (Cell cell in _selectedCells)
                {
                    int actualAmount = Mathf.FloorToInt(amount);
                    if (SandboxManager.Instance.EntityManager.TryIntroduceEntityToCell(cell.Column, cell.Row, _selectedEntityIndex, actualAmount))
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
