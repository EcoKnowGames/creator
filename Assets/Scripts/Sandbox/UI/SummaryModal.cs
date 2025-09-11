using UnityEngine;
using TMPro;
using Glitchers.EcoKnow.Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.UI
{
    public class SummaryModal : MonoBehaviour
    {
        [Header("Scenario Info")]
        [SerializeField] private TMP_Text _scenarioTitle;

        [Header("Objective Results")]
        [SerializeField] private ObjectiveResultWidget _objectiveWidgetPrefab;
        [SerializeField] private Transform _objectiveContainer;

        [Header("Score")]
        [SerializeField] private TMP_Text _scoreText;

        protected RectTransform[] RectTransforms => this.GetComponentsInChildren<RectTransform>();

        private const string LogChannel = "[SummaryModal]";

        public void ShowModal()
        {
            this.gameObject.SetActive(true);
            SetupResults();
            StartCoroutine(RefreshLayout());
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

            //TODO: WebGL support for data export will require some additional js
#endif
        }

        private IEnumerator RefreshLayout()
        {
            yield return new WaitForEndOfFrame();

            if ((RectTransforms != null) && (RectTransforms.Length > 0))
            {
                foreach (RectTransform transform in RectTransforms)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
                }
            }
        }

        #region Win Conditions
        private void SetupTitle()
        {
            string title = ScenarioLoader.Instance.LastPlayedScenario == null ? "Scenario Results" : ScenarioLoader.Instance.LastPlayedScenario.Name;
            if (_scenarioTitle != null)
            {
                _scenarioTitle.text = title;
            }
        }

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

            SetupTitle();
            SetupScore();
        }

        private void SetupScore()
        {
            int totalScore = SandboxManager.Instance.WinConditions == null ? 0 : SandboxManager.Instance.WinConditions.Sum(x => x.Score);

            if (_scoreText != null)
            {
                _scoreText.text = totalScore.ToString();
            }
        }
        #endregion
    }
}
