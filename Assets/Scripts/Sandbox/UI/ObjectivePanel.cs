using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class ObjectivePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _populationTotals;
        [SerializeField] private TMP_Text _winConditions;

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
                        //string success = winCondition.Completed ? winCondition.Completed.ToString() : string.Format($"{winCondition.ConsecutiveSuccesses}/{winCondition.requiredRounds}");

                        string resultStr = string.Empty;
                        foreach(WinCondition.Result result in winCondition.Results)
                        {
                            resultStr += result.ToString() + " / ";
                        }

                        _winConditions.text += string.Format($"{winCondition.title} / Results: {resultStr}\n");
                    }
                }
            }
        }

        public void OnEntityUpdated(int column, int row, int id)
        {
            UpdatePopulations();
            UpdateWinConditions();
        }
    }
}
