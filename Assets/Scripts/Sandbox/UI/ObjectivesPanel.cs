using System;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivesPanel : MonoBehaviour
    {
        [Header("Widgets")]
        [SerializeField] private ObjectiveWidget _widgetPrefab;
        [SerializeField] private Transform _widgetContainer;

        private const string LogChannel = "[ObjectivesPanel]";

        public void Init(WinCondition[] winConditions, EntityManager entityManager)
        {
            if ((_widgetPrefab == null) || (_widgetContainer == null))
            {
                Debug.LogError($"{LogChannel} Failed setup, Widget prefab or Container is null!");
                return;
            }

            //Remove old objectives
            foreach (Transform child in _widgetContainer)
            {
                Destroy(child.gameObject);
            }

            //Instantiate new ones
            foreach (WinCondition condition in winConditions)
            {
                ObjectiveWidget widget = Instantiate(_widgetPrefab, _widgetContainer);
                widget.ResultsTracker.Init(null, -1, -1); //Not ideal but works for now

                switch ((WinCondition.TargetType)condition.TypeIndex)
                {
                    case (WinCondition.TargetType.Entity):
                        {
                            if (condition.TargetEntity != null)
                                widget.SetEntity(condition.TargetIndex, condition.TargetEntity);
                            break;
                        }
                    case (WinCondition.TargetType.Item):
                        {
                            if (condition.TargetItem != null)
                                widget.SetItem(condition.TargetIndex, condition.TargetItem);
                            break;
                        }
                    case (WinCondition.TargetType.Currency):
                        {
                            widget.SetCurrency();
                            break;
                        }
                    default:
                        {
                            Debug.LogError($"{LogChannel} Failed setup, Type not recognised!");
                            break;
                        }
                }
            }
        }

        public void UpdateWinConditions(int currentRound, int maxRounds)
        {
            if (_widgetContainer == null)
            {
                Debug.LogError($"{LogChannel} Failed to update Win Conditions, Widget Container is null!");
                return;
            }

            if (SandboxManager.Instance != null)
            {
                foreach(WinCondition condition in SandboxManager.Instance.WinConditions)
                {
                    ObjectiveWidget widget = GetWidgetByIndex(condition.Type, condition.TargetIndex);
                    if (widget != null)
                    {
                        widget.UpdateObjective(currentRound, maxRounds);
                    }
                }
            }
        }

        private ObjectiveWidget GetWidgetByIndex(WinCondition.TargetType type, int index)
        {
            if (_widgetContainer != null)
            {
                ObjectiveWidget[] widgets = _widgetContainer.GetComponentsInChildren<ObjectiveWidget>();
                if ((widgets != null) && (widgets.Count() > 0))
                {
                    return widgets.FirstOrDefault(x => x.Type == type && x.TargetIndex == index);
                }
            }

            return null;
        }

        private ObjectiveWidget GetWidgetById(WinCondition.TargetType type, string id)
        {
            int index = -1;

            switch(type)
            {
                case (WinCondition.TargetType.Entity):
                    {
                        if (SandboxManager.Instance.EntityManager != null)
                            index = SandboxManager.Instance.EntityManager.GetEntityIndex(id);
                        break;
                    }
                case (WinCondition.TargetType.Item):
                    {
                        if (SandboxManager.Instance.PlayerInventory != null)
                            index = SandboxManager.Instance.PlayerInventory.GetItemIndex(id);
                        break;
                    }
                case (WinCondition.TargetType.Currency):
                    {
                        index = 0;
                        break;
                    }
                default:
                    return null;
            }

            //Try find widget
            if (index >= 0)
            {
                if (_widgetContainer != null)
                {
                    ObjectiveWidget[] widgets = _widgetContainer.GetComponentsInChildren<ObjectiveWidget>();
                    if ((widgets != null) && (widgets.Count() > 0))
                    {
                        return widgets.FirstOrDefault(x => x.Type == type && x.TargetIndex == index);
                    }
                }
            }

            return null;
        }

        public void OnEntityUpdated(int column, int row, int id)
        {
            ObjectiveWidget widget = GetWidgetByIndex(WinCondition.TargetType.Entity, id);
            if (widget != null)
            {
                widget.OnQuantityUpdated();
            }
        }

        public void OnInventoryUpdated(string id, int amount)
        {
            bool isCurrency = id.Equals(PlayerInventory.CurrencyID, StringComparison.OrdinalIgnoreCase);
            ObjectiveWidget widget = GetWidgetById(isCurrency ? WinCondition.TargetType.Currency : WinCondition.TargetType.Item, id);
            if (widget != null)
            {
                widget.OnQuantityUpdated();
            }
        }
    }
}
