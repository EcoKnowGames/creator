using TMPro;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectiveResultWidget : MonoBehaviour
    {
        [Header("Widget")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Entity")]
        [SerializeField] private EntityIcon _entityIcon;

        [Header("Check")]
        [SerializeField] private GameObject _checkIcon;
        [SerializeField] private GameObject _crossIcon;

        [Header("Objective Info")]
        [SerializeField] private TMP_Text _objectiveTitle;
        [SerializeField] private TMP_Text _objectiveDescription;
        [SerializeField] private TMP_Text _scoreText;

        [Header("Results Tracker")]
        [SerializeField] private ResultsTracker _resultsTracker;
        public ResultsTracker ResultsTracker => _resultsTracker;

        private WinCondition.TargetType _type;
        public WinCondition.TargetType Type => _type;

        private int _targetIndex; //Safety
        public int TargetIndex => _targetIndex;

        private const string LogChannel = "[ObjectiveResultWidget]";

        public void Init(WinCondition winCondition)
        {
            SetObjectiveInformation(winCondition);

            switch (winCondition.Type)
            {
                case (WinCondition.TargetType.Entity):
                {
                    SetEntity(winCondition.TargetIndex, winCondition.TargetEntity);
                    break;
                }
                case (WinCondition.TargetType.Item):
                {
                    SetItem(winCondition.TargetIndex, winCondition.TargetItem);
                    break;
                }
                case (WinCondition.TargetType.Currency):
                {
                    SetCurrency();
                    break;
                }
                default:
                    break;
            }

            //If we haven't succeeded, we automatically fail
            if ((winCondition.GetLatestResult() != WinCondition.Result.STREAK) && (winCondition.GetLatestResult() != WinCondition.Result.IN_RANGE))
            {
                SetFailed();
            }
            else
            {
                SetSuccess();
            }

            _resultsTracker?.Init(winCondition.Results, SandboxManager.Instance.MaxRounds, SandboxManager.Instance.MaxRounds);
        }

        private void SetEntity(int index, Entity entity)
        {
            if (entity == null)
            {
                Debug.LogError($"{LogChannel} Failed setup, Entity is null!");
                return;
            }

            _targetIndex = index;
            _type = WinCondition.TargetType.Entity;

            if (_entityIcon != null)
            {
                _entityIcon.SetEntity(entity);
            }
        }

        private void SetItem(int index, Item itemDef)
        {
            if (itemDef == null)
            {
                Debug.LogError($"{LogChannel} Failed setup, Item is null!");
                return;
            }

            _targetIndex = index;
            _type = WinCondition.TargetType.Item;

            if (_entityIcon != null)
            {
                _entityIcon.SetItem(itemDef);
            }
        }
        private void SetCurrency()
        {
            if (_entityIcon != null)
            {
                _entityIcon.SetCurrency();
            }

            _targetIndex = 0;
            _type = WinCondition.TargetType.Currency;
        }


        private void SetObjectiveInformation(WinCondition condition)
        {
            if (_objectiveTitle != null)
            {
                _objectiveTitle.text = condition.title;
            }

            if (_objectiveDescription != null)
            {
                _objectiveDescription.text = condition.description;
            }

            SetScore(condition.Score);
        }

        private void SetScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = score.ToString();
            }
        }

        private void SetSuccess()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1.0f;
            }

            _checkIcon?.SetActive(true);
            _crossIcon?.SetActive(false);
        }

        private void SetFailed()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0.4f;
            }

            _checkIcon?.SetActive(false);
            _crossIcon?.SetActive(true);
        }
    }
}
