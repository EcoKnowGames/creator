using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ResultsTracker : MonoBehaviour
    {
        [SerializeField] private Transform _resultsContainer;
        [SerializeField] private int _maxResultsWidgets = 10;
        [SerializeField] private int _maxFutureRoundCount = 2;

        private ResultsWidget[] _resultWidgets => this.GetComponentsInChildren<ResultsWidget>();
        public ResultsWidget ActiveWidget => _resultWidgets == null ? null : _resultWidgets.FirstOrDefault(x => x.IsActive);

        private const string LogChannel = "[ResultsTracker]";

        public void ResetResultsTrack()
        {
            for (int i = 0; i < _maxResultsWidgets; i++)
            {
                ResultsWidget widget = _resultWidgets[i];
                widget.SetState(i == 0 ? (int)ResultsWidget.State.ACTIVE : (int)ResultsWidget.State.FUTURE);
            }
        }

        public void UpdateResultsTrack(List<WinCondition.Result> results, int currentRound, int maxRounds)
        {
            if ((maxRounds <= 0) || (currentRound < 0) || (currentRound > maxRounds))
            {
                Debug.LogWarning($"{LogChannel} Unable to update Results Tracker, currentRound [{currentRound}] and/or maxRounds [{maxRounds}] are invalid");
                return;
            }

            if (_resultWidgets != null)
            {
                bool failed = results.Contains(WinCondition.Result.FAILED);

                int roundsRemaining = maxRounds - currentRound;
                int futureRoundCount = Mathf.Min(roundsRemaining, _maxFutureRoundCount);
                if (!failed)
                {
                    futureRoundCount += 1; //+1 for the Active Round
                }

                int displayedResultCount = Mathf.Clamp(_maxResultsWidgets - futureRoundCount, 0, results.Count);
                //Debug.Log("Displayed results: " + displayedResultCount);

                List<WinCondition.Result> trimmedResults = results.GetRange(results.Count - displayedResultCount, displayedResultCount);

                for (int i = 0; i < _maxResultsWidgets; i++)
                {
                    ResultsWidget widget = _resultWidgets[i];
                    if (i < displayedResultCount)
                    {
                        widget?.SetState((int)trimmedResults[i]);
                    }
                    else if (i == displayedResultCount)
                    {
                        //Don't show active state if win condition has been failed
                        if (!results.Contains(WinCondition.Result.FAILED))
                        {
                            widget?.SetState((int)ResultsWidget.State.ACTIVE);
                        }
                        else
                        {
                            widget?.SetState((int)ResultsWidget.State.FUTURE);
                        }
                    }
                    else
                    {
                        widget?.SetState((int)ResultsWidget.State.FUTURE);
                    }
                }
            }
        }


        public void UpdateActiveWidget(int population, float lowerLimit, float upperLimit)
        {
            if (ActiveWidget != null)
            {
                ActiveWidget.SetActiveQuantity(population);
                ActiveWidget.SetRange(population, lowerLimit, upperLimit);
            }

            RefreshTracker();
        }

        public void RefreshTracker()
        {
            StartCoroutine(RefreshLayout());
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            //Now force update
            for (int i = 0; i < _maxResultsWidgets; i++)
            {
                //Force update layout
                ResultsWidget widget = _resultWidgets[i];
                RectTransform widgetRect = widget.GetComponent<RectTransform>();
                if (widgetRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(widgetRect);
                }
            }

            if (_resultsContainer != null)
            {
                RectTransform resultsRect = _resultsContainer.GetComponent<RectTransform>();
                if (resultsRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(resultsRect);
                }
            }
        }
    }
}
