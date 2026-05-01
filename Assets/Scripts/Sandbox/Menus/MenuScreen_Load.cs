using UnityEngine;
using System.Runtime.InteropServices;
using TMPro;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Load : MenuScreen
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ReadClipboardText(string gameObjectName, string callbackMethod);
#endif

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

        public override void Hide()
        {
            ScenarioLoader.HideLoadDialog(); //If we exit this screen, hide the dialog
            base.Hide();
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

        public void OnPasteFromClipboardPressed()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ReadClipboardText(gameObject.name, nameof(OnClipboardTextReceived));
#else
            // Fallback for editor/standalone: read from system clipboard
            string text = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(text))
            {
                ValidateJson(text);
            }
#endif
        }

        // Called from JavaScript via SendMessage
        public void OnClipboardTextReceived(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                ValidateJson(text);
            }
            else
            {
                Debug.LogWarning("[MenuScreen_Load] Clipboard was empty or read failed");
                if (_inputFeedbackText != null)
                {
                    _inputFeedbackText.alpha = 1.0f;
                }
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
