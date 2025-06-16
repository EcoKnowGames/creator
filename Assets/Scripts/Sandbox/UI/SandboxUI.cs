using System;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private ObjectivePanel _objectivePanel;
        [SerializeField] private PlayerToolbar _playerToolbar;
        [SerializeField] private ResultsModal _resultsModal;

        public void Init()
        {
            _objectivePanel?.Init();
            _playerToolbar?.Init();
            _resultsModal?.HideModal();
        }


        public void OnGameEnded(SandboxManager.Result result)
        {
            _resultsModal?.ShowModal(result.ToString());
        }

        public void OnNewRoundStarted(int currentRound, int maxRounds, int actions)
        {
            _roundIndicator?.UpdateRoundCounter(currentRound, maxRounds);
            _objectivePanel?.UpdatePopulations();
            _playerToolbar?.UpdateActionsRemaining(actions);
        }
    }
}
