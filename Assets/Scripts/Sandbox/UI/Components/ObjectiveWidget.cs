using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectiveWidget : MonoBehaviour
    {
        private CanvasGroup _canvasGroup => this.GetComponent<CanvasGroup>();

        [Header("Entity")]
        [SerializeField] private Image _entityBackground;
        [SerializeField] private Image _entityIcon;

        [Header("Round Target")]
        [SerializeField] private TMP_Text _roundTargetText;
        [SerializeField] private GameObject _roundTargetCheck;

        [Header("Results Tracker")]
        //[SerializeField] private ResultsWidget _resultsWidgetPrefab;
        [SerializeField] private Transform _resultsContainer;
        [SerializeField] private int _maxResultsWidgets = 10;
        [SerializeField] private int _maxFutureRoundCount = 2;

        private ResultsWidget[] _resultWidgets => this.GetComponentsInChildren<ResultsWidget>();
        public ResultsWidget ActiveWidget => _resultWidgets == null ? null : _resultWidgets.FirstOrDefault(x => x.IsActive);

        private int _entityIndex; //Safety
        public int EntityIndex => _entityIndex;

        private const string LogChannel = "[ObjectiveWidget]";

        public void SetEntity(int index, Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError($"{LogChannel} Failed setup, Entity is null!");
                return;
            }

            _entityIndex = index;

            //Set icon
            if (_entityIcon != null)
            {
                Sprite resource = Resources.Load<Sprite>(entity.Icon);
                if (resource != null)
                {
                    _entityIcon.sprite = resource;
                }
                else
                {
                    Debug.LogError($"{LogChannel} Failed to find icon for entity at path {entity.Icon}!");
                }
            }

            //Set Colour
            if (_entityBackground != null)
            {
                Color colour = Color.white;
                ColorUtility.TryParseHtmlString("#" + entity.Colour, out colour);
                _entityBackground.color = colour;
            }
        }

        public void UpdateObjective(WinCondition condition, int currentRound, int maxRound)
        {
            if (condition.Completed)
            {
                //Set Complete
                SetComplete();
            }
            else
            {
                //Check for failure
                if ((condition.Results != null) && (condition.Results.Count > 0))
                {
                    if (condition.Results.Last() == WinCondition.Result.FAILED)
                    {
                        SetFailed();
                    }
                    else
                    {
                        SetInProgress(condition.requiredRounds);
                    }
                }
            }

            if ((condition.Results != null) && (condition.Results.Count > 0))
            {
                UpdateResultsTrack(condition.Results, currentRound, maxRound);
            }

            UpdatePopulation();
        }

        public void UpdatePopulation()
        {
            if (ActiveWidget != null)
            {
                if (SandboxManager.Instance.EntityManager != null)
                {
                    int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(EntityIndex);
                    ActiveWidget.SetActiveQuantity(population);
                }
            }

            StartCoroutine(RefreshLayout());
        }

        #region Overall State
        private void SetComplete()
        {
            if (_roundTargetCheck != null)
            {
                _roundTargetCheck.SetActive(true);
            }
        }

        private void SetInProgress(int rounds)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            if (_roundTargetText != null)
            {
                _roundTargetText.text = rounds.ToString();
            }

            if (_roundTargetCheck != null)
            {
                _roundTargetCheck.SetActive(false);
            }
        }

        private void SetFailed()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0.4f;
            }

            if (_roundTargetText != null)
            {
                _roundTargetText.text = "FAIL";
            }

            if (_roundTargetCheck != null)
            {
                _roundTargetCheck.SetActive(false);
            }
        }
        #endregion

        #region Results Tracker
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
                //Check failure
                if (results.Last() == WinCondition.Result.FAILED)
                {
                    SetFailed();
                }

                int roundsRemaining = maxRounds - currentRound;
                int futureRoundCount = Mathf.Min(roundsRemaining, _maxFutureRoundCount);

                int displayedResultCount = Mathf.Clamp(_maxResultsWidgets - futureRoundCount, 0, results.Count);
                //Debug.Log("Displayed results: " + displayedResultCount);

                List<WinCondition.Result> trimmedResults = results.GetRange(results.Count - displayedResultCount, displayedResultCount);

                for (int i = 0; i < _maxResultsWidgets; i++)
                {
                    //WinCondition.Result previousResult = i > 0 ? results[i-1] : WinCondition.Result.NOT_STARTED;

                    ResultsWidget widget = _resultWidgets[i];
                    if (i < displayedResultCount)
                    {
                        widget?.SetState((int)trimmedResults[i]);
                    }
                    else if (i == displayedResultCount)
                    {
                        widget?.SetState((int)ResultsWidget.State.ACTIVE);
                    }
                    else
                    {
                        widget?.SetState((int)ResultsWidget.State.FUTURE);
                    }
                }
            }
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
        #endregion
    }
}
