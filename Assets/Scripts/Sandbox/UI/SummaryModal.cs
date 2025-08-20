using UnityEngine;
using TMPro;
using Glitchers.EcoKnow.Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SummaryModal : MonoBehaviour
    {
        [Header("Objective Results")]
        [SerializeField] private ObjectiveResultWidget _objectiveWidgetPrefab;
        [SerializeField] private Transform _objectiveContainer;

        private const string LogChannel = "[SummaryModal]";

        public void ShowModal()
        {
            SetupResults();
            this.gameObject.SetActive(true);
        }

        public void HideModal()
        {
            this.gameObject.SetActive(false);
        }

        public void OnReplayPressed()
        {
            SandboxManager.Instance.ReplayCurrentScenario();
        }

        public void OnQuitPressed()
        {
            Application.Quit();
        }

        public void OnExportDataPressed()
        {
#if !UNITY_WEBGL
            Data.DataManager.Instance.ShowSaveDialog(null, null);

            //TODO(caspar): Do we want to allow data export on WebGL? This will likely require some js
#endif
        }

        #region Win Conditions
        private void SetupResults()
        {
            if ((_objectiveWidgetPrefab == null) || (_objectiveContainer == null))
            {
                Debug.LogError($"{LogChannel} Failed setup, Widget prefab or Container is null!");
                return;
            }

            //Remove old objectives
            foreach (Transform child in _objectiveContainer)
            {
                Destroy(child.gameObject);
            }

            //Instantiate new ones
            List<WinCondition> winConditions = SandboxManager.Instance.WinConditions.OrderByDescending(x => x.Completed).ToList();

            foreach (WinCondition condition in winConditions)
            {
                ObjectiveResultWidget widget = Instantiate(_objectiveWidgetPrefab, _objectiveContainer);
                if (widget != null)
                {
                    widget.Init(condition);
                }
            }
        }

        #endregion
    }
}
