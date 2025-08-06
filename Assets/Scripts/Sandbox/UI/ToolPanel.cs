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

        private const string LogChannel = "[ToolPanel]";

        public void Init()
        {
            HideToolbar();
        }


        public void ShowToolbar(int entityIndex)
        {
            SetEntity(entityIndex);
            this.gameObject.SetActive(true);
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
            if (_harvestButton != null)
            {
                _harvestButton.interactable = canHarvest;
            }

            if (_introduceButton != null)
            {
                _introduceButton.interactable = canIntroduce;
            }
        }
    }
}

