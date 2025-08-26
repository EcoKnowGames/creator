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
        [SerializeField] private EntityIcon _entityIcon;

        [Header("Round Target")]
        [SerializeField] private TMP_Text _roundTargetText;
        [SerializeField] private GameObject _roundTargetCheck;

        [Header("Results Tracker")]
        [SerializeField] private ResultsTracker _resultsTracker;
        public ResultsTracker ResultsTracker => _resultsTracker;

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

           if (_entityIcon != null)
            {
                _entityIcon.SetEntity(entity);
            }
        }

        public void UpdateObjective(int currentRound, int maxRound)
        {
            WinCondition condition = GetWinCondition();
            if (condition != null)
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
                    else
                    {
                        SetInProgress(condition.requiredRounds);
                    }
                }

                if ((condition.Results != null) && (condition.Results.Count > 0))
                {
                    _resultsTracker?.UpdateResultsTrack(condition.Results, currentRound, maxRound);
                }

                UpdatePopulation(condition);
            }
        }

        public void OnEntityUpdated()
        {
            WinCondition condition = GetWinCondition();
            if (condition != null)
            {
                UpdatePopulation(condition);
            }
        }

        public void UpdatePopulation(WinCondition condition)
        {
            if (condition.GetLatestResult() == WinCondition.Result.FAILED)
            {
                return;
            }

            if (SandboxManager.Instance.EntityManager != null)
            {
                int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(EntityIndex);
                _resultsTracker?.UpdateActiveWidget(population, condition.lowerLimit, condition.upperLimit);
            }
        }


        private WinCondition GetWinCondition()
        {
            if (SandboxManager.Instance.WinConditions != null)
            {
                return SandboxManager.Instance.WinConditions.FirstOrDefault(x => x.EntityIndex == EntityIndex);
            }

            return null;
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
    }
}
