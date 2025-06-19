using System;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private ObjectivePanel _objectivePanel;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ResultsModal _resultsModal;

        public void Init()
        {
            _objectivePanel?.Init();
            _inventoryPanel?.Init();
            _playerToolbar?.Init();
            _resultsModal?.HideModal();

            foreach(RectTransform child in this.GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            }
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
