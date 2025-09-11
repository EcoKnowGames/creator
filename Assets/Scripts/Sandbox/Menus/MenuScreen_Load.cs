using UnityEngine;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Load : MenuScreen
    {
        [Header("Raw JSON Input")]
        [SerializeField] private TMP_InputField _rawJsonInput;
        [SerializeField] private CanvasGroup _inputFeedbackText;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (_inputFeedbackText != null)
            {
                _inputFeedbackText.alpha = 0.0f;
            }
        }

        public void OnLoadFromFilePressed()
        {
            ScenarioLoader.ShowLoadDialog(OnScenarioLoaded, null);
        }

        public void OnLoadFromJsonPressed()
        {
            if (_rawJsonInput != null)
            {
                ValidateJson(_rawJsonInput.text);
            }
        }

        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.SELECT_SCENARIO);
        }

        private void OnScenarioLoaded(Scenario scenario)
        {
            if (scenario == null)
            {
                if (_inputFeedbackText != null)
                {
                    _inputFeedbackText.alpha = 1.0f;
                }
            }
            else
            {
                MainMenuController?.SetScreen(MainMenuController.Screen.SETUP_SCENARIO);
            }
        }

        private void ValidateJson(string rawJson)
        {
            ScenarioConfig config = ScenarioLoader.LoadConfig(rawJson);
            if (config != null)
            {
                ScenarioLoader.Instance.SetLoadedConfig(config);
                MainMenuController.SetScreen(MainMenuController.Screen.SETUP_SCENARIO);
            }
            else
            {
                if (_inputFeedbackText != null)
                {
                    _inputFeedbackText.alpha = 1.0f;
                }
            }
        }
    }
}
