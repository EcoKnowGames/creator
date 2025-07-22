using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class EntityPanel : MonoBehaviour
    {
        [SerializeField] private Transform _entityButtonContainer;
        [SerializeField] private Button _entityButtonPrefab;

        public Action<int> onEntitySelected;

        public void Init(Entity[] entities)
        {
            if (entities == null)
            {
                return;
            }

            SetupEntityList(entities);
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
                Button button = Instantiate(_entityButtonPrefab, _entityButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = entities[i].ID;
                }

                int entityIndex = i;
                button.onClick.AddListener(delegate {
                    onEntitySelected?.Invoke(entityIndex);
                });
            }

            if (this.GetComponent<RectTransform>() != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.GetComponent<RectTransform>());
            }
        }
    }
}
