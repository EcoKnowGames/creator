using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _populationTotals;

        public void Init()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.entityEvents.OnEntityHarvested += OnEntityUpdated;
                SandboxManager.Instance.EntityManager.entityEvents.OnEntityIntroduced += OnEntityUpdated;
            }
        }

        public void OnDestroy()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.entityEvents.OnEntityHarvested -= OnEntityUpdated;
                SandboxManager.Instance.EntityManager.entityEvents.OnEntityIntroduced -= OnEntityUpdated;
            }
        }

        public void UpdatePopulations()
        {
            if (_populationTotals == null)
            {
                return;
            }

            _populationTotals.text = string.Empty;

            if (SandboxManager.Instance.EntityManager != null)
            {
                for (int i = 0; i < SandboxManager.Instance.EntityManager.EntityTypeCount; i++)
                {
                    Entity entityType = SandboxManager.Instance.EntityManager.GetEntityType(i);
                    if (entityType != null)
                    {
                        int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(i);
                        _populationTotals.text += string.Format($"{population} / {entityType.ID}\n");
                    }
                }
            }
        }

        protected void OnEntityUpdated(int column, int row, int id)
        {
            UpdatePopulations();
        }
    }
}
