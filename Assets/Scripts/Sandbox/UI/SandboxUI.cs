using System;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SandboxUI : MonoBehaviour
    {
        [SerializeField] private RoundIndicator _roundIndicator;
        [SerializeField] private PlayerToolbar _playerToolbar;

        public void Init()
        {
            _playerToolbar?.Init();
        }

        internal void OnNewRoundStarted(int currentRound, int maxRounds, int actions)
        {
            _roundIndicator?.UpdateRoundCounter(currentRound, maxRounds);
            _playerToolbar?.UpdateActionsRemaining(actions);
        }
    }
}
