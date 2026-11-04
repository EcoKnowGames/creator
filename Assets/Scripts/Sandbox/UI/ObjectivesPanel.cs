using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivesPanel : MonoBehaviour
    {
        [Header("Widgets")]
        [SerializeField] private ObjectiveWidget _widgetPrefab;
        [SerializeField] private Transform _widgetContainer;

        //One widget per win condition; each widget holds its own condition
        private readonly List<ObjectiveWidget> _widgets = new List<ObjectiveWidget>();

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
            _widgets.Clear();

            //Instantiate new ones
            foreach (WinCondition condition in winConditions)
            {
                ObjectiveWidget widget = Instantiate(_widgetPrefab, _widgetContainer);
                widget.ResultsTracker.Init(null, -1, -1); //Not ideal but works for now
                widget.Init(condition);
                _widgets.Add(widget);
            }
        }

        public void UpdateWinConditions(int currentRound, int maxRounds)
        {
            if (_widgetContainer == null)
            {
                Debug.LogError($"{LogChannel} Failed to update Win Conditions, Widget Container is null!");
                return;
            }

            foreach (ObjectiveWidget widget in _widgets)
            {
                if (widget != null)
                {
                    widget.UpdateObjective(currentRound, maxRounds);
                }
            }
        }

        //Refresh every widget whose condition matches the target (several conditions may share one)
        private void RefreshWidgetsTargeting(WinCondition.TargetType type, string id)
        {
            foreach (ObjectiveWidget widget in _widgets)
            {
                if ((widget != null) && (widget.WinCondition != null) && widget.WinCondition.Targets(type, id))
                {
                    widget.OnQuantityUpdated();
                }
            }
        }

        public void OnEntityUpdated(int column, int row, int index)
        {
            //Convert the runtime (matrix-sorted) index to the entity's ID
            EntityManager entityManager = SandboxManager.Instance != null ? SandboxManager.Instance.EntityManager : null;
            if (entityManager == null)
            {
                return;
            }

            Entity entity = entityManager.GetEntityType(index);
            if (entity != null)
            {
                RefreshWidgetsTargeting(WinCondition.TargetType.Entity, entity.ID);
            }
        }

        public void OnInventoryUpdated(string id, int amount)
        {
            bool isCurrency = id.Equals(PlayerInventory.CurrencyID, StringComparison.OrdinalIgnoreCase);
            RefreshWidgetsTargeting(isCurrency ? WinCondition.TargetType.Currency : WinCondition.TargetType.Item, id);
        }
    }
}
