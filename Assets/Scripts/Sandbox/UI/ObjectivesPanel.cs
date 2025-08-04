using UnityEngine;
using TMPro;
using System.Linq;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivesPanel : MonoBehaviour
    {
        [Header("Widgets")]
        [SerializeField] private ObjectiveWidget _widgetPrefab;
        [SerializeField] private Transform _widgetContainer;

        [Header("Old")]
        [SerializeField] private TMP_Text _populationTotals;
        [SerializeField] private TMP_Text _winConditions;

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

                //Setup entity
                Entity entity = entityManager.GetEntityType(condition.EntityIndex);
                if (entity != null)
                {
                    widget.SetEntity(condition.EntityIndex, entity);
                }
                else
                {
                    Debug.LogError($"{LogChannel} Failed setup, Entity is null!");
                    continue;
                }

                widget.SetRoundTarget(condition.requiredRounds);
                widget.SetComplete(condition.Completed);
            }
        }

        public void UpdateWinConditions()
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
                    ObjectiveWidget widget = GetWidgetForEntity(condition.EntityIndex);
                    if (widget != null)
                    {
                        widget.SetComplete(condition.Completed); //TODO(caspar): Rethink this function?
                        //TODO(caspar): Update track
                    }
                }
            }
        }

        private ObjectiveWidget GetWidgetForEntity(int index)
        {
            if (_widgetContainer != null)
            {
                ObjectiveWidget[] widgets = _widgetContainer.GetComponentsInChildren<ObjectiveWidget>();
                if ((widgets != null) && (widgets.Count() > 0))
                {
                    return widgets.FirstOrDefault(x => x.EntityIndex == index);
                }
            }

            return null;
        }

        public void OnEntityUpdated(int column, int row, int id)
        {
            UpdateWinConditions();
        }
    }
}
