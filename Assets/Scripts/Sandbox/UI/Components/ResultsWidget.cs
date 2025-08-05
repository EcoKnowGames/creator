using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ResultsWidget : MonoBehaviour
    {
        //This matches the states in WinCondition
        //but includes extra states like FUTURE that are specific to the tracker
        public enum State
        {
            NOT_STARTED = 0,
            IN_RANGE = 1,
            STREAK = 2,
            GRACE = 3,
            FAILED = 4,
            ACTIVE = 5,
            FUTURE = 6
        };

        [Header("States")]
        [SerializeField] private GameObject _notStartedState;
        [SerializeField] private GameObject _inRangeState;
        [SerializeField] private GameObject _streakState;
        [SerializeField] private GameObject _graceState;
        [SerializeField] private GameObject _failedState;
        [SerializeField] private GameObject _activeState;
        [SerializeField] private GameObject _futureState;

        //TODO(caspar): Active range
        [Header("Active")]
        [SerializeField] private TMP_Text _activeText;

        private State _currentState;
        public bool IsActive => _currentState == State.ACTIVE;

        public void SetState(int state)
        {
            _currentState = (State)state;
            HideAllStates();

            switch (_currentState)
            {
                case (State.FAILED):
                    {
                        _failedState?.SetActive(true);
                        break;
                    }
                case (State.GRACE):
                    {
                        _graceState?.SetActive(true);
                        break;
                    }
                case (State.STREAK):
                    {
                        _streakState?.SetActive(true);
                        break;
                    }
                case (State.IN_RANGE):
                    {
                        _inRangeState?.SetActive(true);
                        break;
                    }
                case (State.ACTIVE):
                    {
                        _activeState?.SetActive(true);
                        break;
                    }
                case (State.FUTURE):
                    {
                        _futureState?.SetActive(true);
                        break;
                    }
                case (State.NOT_STARTED):
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
            _activeState?.SetActive(false);
            _futureState?.SetActive(false);
        }

        public void SetActiveQuantity(int quantity)
        {
            if (_activeText != null)
            {
                _activeText.text = FormatQuantity(quantity);
            }
        }

        private string FormatQuantity(int quantity)
        {
            if (quantity >= 1000000)
            {
                float roundedQuantity = Mathf.Floor(((float)quantity / 100000f) * 10f) / 10f;
                return roundedQuantity.ToString("0.#") + "M";
            }
            else if (quantity >= 1000)
            {
                float roundedQuantity = Mathf.Floor(((float)quantity / 1000f) * 10f) / 10f;
                return roundedQuantity.ToString("0.#") + "k";
            }

            return quantity.ToString();
        }
    }
}
