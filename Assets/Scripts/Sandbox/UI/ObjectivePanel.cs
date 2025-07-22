using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _populationTotals;
        [SerializeField] private TMP_Text _winConditions;

        public void Init()
        {
            if (SandboxManager.Instance.EntityManager != null)
            {
                SandboxManager.Instance.EntityManager.OnEntityHarvested += OnEntityUpdated;
                SandboxManager.Instance.EntityManager.OnEntityIntroduced += OnEntityUpdated;
            }
        }

        public void OnDestroy()
        {
            //if (SandboxManager.Instance.EntityManager != null)
            //{
                //SandboxManager.Instance.EntityManager.OnEntityHarvested -= OnEntityUpdated;
                //SandboxManager.Instance.EntityManager.OnEntityIntroduced -= OnEntityUpdated;
            //}
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
                Entity[] entityList = SandboxManager.Instance.EntityManager.GetEntityTypeList();
                if (entityList != null)
                {
                    for (int i = 0; i < entityList.Length; i++)
                    {
                        Entity entityType = entityList[i];
                        if (entityType != null)
                        {
                            int population = SandboxManager.Instance.EntityManager.GetTotalPopulationOfEntityType(i);
                            _populationTotals.text += string.Format($"{population} / {entityType.ID}\n");
                        }
                    }
                }
            }
        }

        public void UpdateWinConditions()
        {
            if (_winConditions == null)
            {
                return;
            }

            _winConditions.text = string.Empty;

            if (SandboxManager.Instance != null)
            {
                for (int i = 0; i < SandboxManager.Instance.WinConditions.Count; i++)
                {
                    WinCondition winCondition = SandboxManager.Instance.WinConditions[i];
                    if (winCondition != null)
                    {
                        string success = winCondition.Completed ? winCondition.Completed.ToString() : string.Format($"{winCondition.ConsecutiveSuccesses}/{winCondition.requiredRounds}"); 
                        _winConditions.text += string.Format($"{winCondition.title} / Completed: {success}\n");
                    }
                }
            }
        }

        protected void OnEntityUpdated(int column, int row, int id)
        {
            UpdatePopulations();
            UpdateWinConditions();
        }
    }
}
