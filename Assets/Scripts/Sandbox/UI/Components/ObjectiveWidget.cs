using System.Linq;
using TMPro;
using UnityEngine;

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

        //The condition this widget displays. Its TargetId is the only target reference; nothing is copied.
        private WinCondition _winCondition;
        public WinCondition WinCondition => _winCondition;

        private const string LogChannel = "[ObjectiveWidget]";

        public void Init(WinCondition condition)
        {
            _winCondition = condition;
            if (condition == null)
            {
                Debug.LogError($"{LogChannel} Failed setup, Win Condition is null!");
                return;
            }

            //Visuals only: a failed lookup leaves the icon unset but the widget stays bound to its own condition
            switch (condition.Type)
            {
                case (WinCondition.TargetType.Entity):
                    {
                        SetEntity(condition.TargetEntity, condition);
                        break;
                    }
                case (WinCondition.TargetType.Item):
                    {
                        SetItem(condition.TargetItem, condition);
                        break;
                    }
                case (WinCondition.TargetType.Currency):
                    {
                        SetCurrency();
                        break;
                    }
                default:
                    {
                        Debug.LogError($"{LogChannel} Failed setup of [{condition.title}], Type not recognised!");
                        break;
                    }
            }
        }

        private void SetEntity(Entity entity, WinCondition condition)
        {
            if (entity == null)
            {
                Debug.LogError($"{LogChannel} Failed setup of [{condition.title}], Entity [{condition.TargetId}] not found!");
                return;
            }

            if (_entityIcon != null)
            {
                _entityIcon.SetEntity(entity);
            }
        }

        private void SetItem(Item itemDef, WinCondition condition)
        {
            if (itemDef == null)
            {
                Debug.LogError($"{LogChannel} Failed setup of [{condition.title}], Item [{condition.TargetId}] not found!");
                return;
            }

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
        }

        public void UpdateObjective(int currentRound, int maxRound)
        {
            WinCondition condition = _winCondition;
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

                UpdateQuantity(condition);
            }
        }

        public void OnQuantityUpdated()
        {
            WinCondition condition = _winCondition;
            if (condition != null)
            {
                UpdateQuantity(condition);
            }
        }

        public void UpdateQuantity(WinCondition condition)
        {
            if (condition.GetLatestResult() == WinCondition.Result.FAILED)
            {
                return;
            }

            int quantity = 0;
            switch (condition.Type)
            {
                case (WinCondition.TargetType.Entity):
                    {
                        EntityManager entityManager = SandboxManager.Instance.EntityManager;
                        if (entityManager != null)
                        {
                            int entityIndex = entityManager.GetEntityIndex(condition.TargetId);
                            if (entityIndex >= 0)
                            {
                                quantity = entityManager.GetTotalPopulationOfEntityType(entityIndex);
                            }
                        }
                        break;
                    }
                case (WinCondition.TargetType.Item):
                    {
                        if (SandboxManager.Instance.PlayerInventory != null)
                        {
                            Item itemDef = SandboxManager.Instance.PlayerInventory.GetItemDef(condition.TargetId);
                            if (itemDef != null)
                            {
                                quantity = SandboxManager.Instance.PlayerInventory.GetAmountHeld(itemDef.ID);
                            }
                        }
                        break;
                    }
                case (WinCondition.TargetType.Currency):
                    {
                        if (SandboxManager.Instance.PlayerInventory != null)
                        {
                            quantity = SandboxManager.Instance.PlayerInventory.GetAmountHeld(PlayerInventory.CurrencyID);
                        }
                        break;
                    }
                default:
                    break;
            }

            _resultsTracker?.UpdateActiveWidget(quantity, condition.lowerLimit, condition.upperLimit);
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
