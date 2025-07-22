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
        [SerializeField] private ResultsModal _resultsModal;

        #region Setup
        public void Init(Entity[] entities)
        {
            //Initialise our components
            _entityPanel?.Init(entities);
            _objectivePanel?.Init();
            _inventoryPanel?.Init();
            _playerToolbar?.Init();
            _resultsModal?.HideModal();

            //Subscribe to UI events
            _entityPanel.onEntitySelected += OnEntitySelected;

            //Force rebuild
            foreach (RectTransform child in this.GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            }
        }
        #endregion

        private void OnEntitySelected(int entityIndex)
        {
            //TODO(caspar): Open the toolbar with the requested entity type
            _playerToolbar?.ShowToolbar(entityIndex);
        }

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
    }
}
