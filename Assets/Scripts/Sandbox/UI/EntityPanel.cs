using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class EntityPanel : MonoBehaviour
    {
        [SerializeField] private Transform _entityButtonContainer;
        [SerializeField] private Button _entityButtonPrefab;

        private int _selectedEntityIndex = -1;
        public int SelectedEntityIndex => _selectedEntityIndex;

        public Action<int> onEntitySelected;
        public Action onEntityDeselected;

        public void Init(Entity[] entities)
        {
            if (entities == null)
            {
                return;
            }

            SetupEntityList(entities);
        }

        public void Cleanup()
        {
            onEntitySelected = null;
            onEntityDeselected = null;
        }

        private void SetupEntityList(Entity[] entities)
        {
            if ((_entityButtonPrefab == null) || (_entityButtonContainer == null))
            {
                return;
            }

            foreach (Transform child in _entityButtonContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < entities.Length; i++)
            {
                int entityIndex = i;

                Button button = Instantiate(_entityButtonPrefab, _entityButtonContainer);
                EntityWidget widget = button.GetComponent<EntityWidget>();
                if (widget != null)
                {
                    widget.SetEntity(entityIndex, entities[i]);
                }

                button.onClick.AddListener(delegate {
                    if (_selectedEntityIndex == entityIndex)
                    {
                        //Deselect
                        DeselectEntity();
                    }
                    else
                    {
                        //Select
                        SelectEntity(entityIndex);
                    }
                });
            }

            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }

        private EntityWidget GetWidgetForEntity(int index)
        {
            if (_entityButtonContainer != null)
            {
                EntityWidget[] widgets = _entityButtonContainer.GetComponentsInChildren<EntityWidget>();
                if ((widgets != null) && (widgets.Count() > 0))
                {
                    return widgets.FirstOrDefault(x => x.EntityIndex == index);
                }
            }

            return null;
        }

        private void UnfocusAllWidgets()
        {
            if (_entityButtonContainer != null)
            {
                foreach (EntityWidget widget in _entityButtonContainer.GetComponentsInChildren<EntityWidget>())
                {
                    widget.Unfocus();
                }
            }
        }

        public void UpdateAllWidgets()
        {
            if (_entityButtonContainer != null)
            {
                foreach (EntityWidget widget in _entityButtonContainer.GetComponentsInChildren<EntityWidget>())
                {
                    widget.UpdateQuantity();
                }
            }
        }

        /*private void OrderWidgets()
        {
            if (_entityButtonContainer != null)
            {

                if (SandboxManager.Instance.EntityManager != null)
                {
                    int population = SandboxManager.Instance.EntityManager.getenti
                    if (_quantityText != null)
                    {
                        _quantityText.text = FormatQuantity(population);
                    }
                }


                EntityWidget[] orderedEntities = _entityButtonContainer.GetComponentsInChildren<EntityWidget>().OrderByDescending(x => x.)
                foreach (EntityWidget widget in _entityButtonContainer.GetComponentsInChildren<EntityWidget>())
                {
                    widget.UpdateQuantity();
                }
            }
        }*/


        private void SelectEntity(int index)
        {
            //Clamp just in case?
            int maxIndex = 0;
            if (SandboxManager.Instance.EntityManager != null)
            {
                maxIndex = SandboxManager.Instance.EntityManager.EntityTypeCount;
            }

            _selectedEntityIndex = Mathf.Clamp(index, 0, maxIndex);

            //Now select our widget
            EntityWidget selected = GetWidgetForEntity(index);
            if (selected != null)
            {
                UnfocusAllWidgets();
                selected.Focus();

                onEntitySelected?.Invoke(index);
            }
            else
            {
                //TODO(caspar): Failed?
            }
        }

        public void DeselectEntity()
        {
            _selectedEntityIndex = -1;
            UnfocusAllWidgets();
            onEntityDeselected?.Invoke();
        }

        public void OnEntityUpdated(int column, int row, int index)
        {
            EntityWidget selected = GetWidgetForEntity(index);
            if (selected != null)
            {
                selected.UpdateQuantity();
            }
        }
    }
}
