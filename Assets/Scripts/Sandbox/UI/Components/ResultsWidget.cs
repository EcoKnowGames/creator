using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ResultsWidget : MonoBehaviour
    {
        //This matches the states in WinCondition
        //but includes extra states like FUTURE that are specific to the tracker
        public enum Result
        {
            NOT_STARTED = 0,
            IN_RANGE = 1,
            STREAK = 2,
            GRACE = 3,
            FAILED = 4,
            FUTURE = 5
        };

        [Header("States")]
        [SerializeField] private GameObject _notStartedState;
        [SerializeField] private GameObject _inRangeState;
        [SerializeField] private GameObject _streakState;
        [SerializeField] private GameObject _graceState;
        [SerializeField] private GameObject _failedState;
        [SerializeField] private GameObject _futureState;

        //TODO(caspar): Active range

        private Result _currentState;

        public void SetState(int state)
        {
            _currentState = (Result)state;
            HideAllStates();

            switch (_currentState)
            {
                case (Result.FAILED):
                    {
                        _failedState?.SetActive(true);
                        break;
                    }
                case (Result.GRACE):
                    {
                        _graceState?.SetActive(true);
                        break;
                    }
                case (Result.STREAK):
                    {
                        _streakState?.SetActive(true);
                        break;
                    }
                case (Result.IN_RANGE):
                    {
                        _inRangeState?.SetActive(true);
                        break;
                    }
                case (Result.FUTURE):
                    {
                        _futureState?.SetActive(true);
                        break;
                    }
                case (Result.NOT_STARTED):
                default:
                    {
                        _notStartedState?.SetActive(true);
                        break;
                    }

            }

        }

        private void HideAllStates()
        {
            _notStartedState?.SetActive(false);
            _inRangeState?.SetActive(false);
            _streakState?.SetActive(false);
            _graceState?.SetActive(false);
            _failedState?.SetActive(false);
            _futureState?.SetActive(false);
        }
    }
}
