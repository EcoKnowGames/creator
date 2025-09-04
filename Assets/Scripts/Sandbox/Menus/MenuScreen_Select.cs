using System.Collections.Generic;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Select : MenuScreen
    {
        [Header("Scenario Buttons")]
        [SerializeField] private Button_ScenarioSelect _scenarioSelectButtonPrefab;
        [SerializeField] private Transform _scenarioSelectContainer;

        public override void Show()
        {
            base.Show();

            if (ScenarioLoader.Instance != null)
            {
                SetupScenarioList(ScenarioLoader.Instance.GetIntegratedScenarioConfigs());
            }
        }

        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.MAIN);
        }

        public void OnLoadOtherScenarioPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.LOAD_SCENARIO);
        }

        private void SetupScenarioList(List<ScenarioConfig> scenarioConfigs)
        {
            if ((scenarioConfigs == null) || (scenarioConfigs.Count <= 0))
            {
                //error
                return;
            }

            //Cleanup old stuff
            foreach (Transform child in _scenarioSelectContainer)
            {
                Destroy(child.gameObject);
            }

            //Add new buttons
            for (int i = 0; i < scenarioConfigs.Count; i++)
            {
                AddScenarioToList(scenarioConfigs[i]);
            }
        }

        private void AddScenarioToList(ScenarioConfig config)
        {
            //Instantiate and set up button
            if (config != null)
            {
                Button_ScenarioSelect button = Instantiate(_scenarioSelectButtonPrefab, _scenarioSelectContainer);
                if (button != null)
                {
                    button.SetScenarioData(button.transform.GetSiblingIndex(), config.Scenario.Name, "TODO", config.Scenario.ActionsPerRound);
                    button.PlayButton.onClick.AddListener(() =>
                    {
                        ScenarioLoader.Instance.SetLoadedConfig(config);
                        MainMenuController.SetScreen(MainMenuController.Screen.SETUP_SCENARIO);
                    });
                }
            }
        }
    }
}
