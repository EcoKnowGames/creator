using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ToolPanel : MonoBehaviour
    {
        [Header("Entity Info")]
        [SerializeField] private TMP_Text _entityNameText;
        [SerializeField] private TMP_Text _entityPopulationText;

        [Header("Buttons")]
        [SerializeField] private Button _harvestButton;
        [SerializeField] private Button _introduceButton;

        [Header("Positioning")]
        [SerializeField] private float entityWidgetXOffset = -70f;

        private const string LogChannel = "[ToolPanel]";

        public void Init()
        {
            HideToolbar();
        }


        public void ShowToolbar(int entityIndex, EntityWidget anchoredWidget)
        {
            SetEntity(entityIndex);
            this.gameObject.SetActive(true);

            AnchorToPosition(anchoredWidget.transform.position, entityWidgetXOffset);
        }

        public void HideToolbar()
        {
            this.gameObject.SetActive(false);
        }

        private void SetEntity(int entityIndex)
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                Entity entity = SandboxManager.Instance.EntityManager.GetEntityType(entityIndex);
                if (entity != null)
                {
                    if (_entityNameText != null)
                    {
                        _entityNameText.text = entity.ID;
                    }

                    SetButtonsInteractable(entity.CanHarvest, entity.CanIntroduce);
                }

                int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(entityIndex);
                if (_entityPopulationText != null)
                {
                    _entityPopulationText.text = population.ToString("n0");
                }
            }
        }

        private void SetButtonsInteractable(bool canHarvest, bool canIntroduce)
        {
            bool hasActionsRemaining = SandboxManager.CanPerformAction();

            if (_harvestButton != null)
            {
                _harvestButton.interactable = hasActionsRemaining && canHarvest;
            }

            if (_introduceButton != null)
            {
                _introduceButton.interactable = hasActionsRemaining && canIntroduce;
            }
        }

        private void AnchorToPosition(Vector3 position, float xOffset = 0f)
        {
            Vector3 finalPosition = position;
            finalPosition.x += xOffset;

            this.transform.position = finalPosition;
        }
    }
}

