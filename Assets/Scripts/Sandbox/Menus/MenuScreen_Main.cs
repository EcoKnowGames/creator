using UnityEngine;
using UnityEngine.UI;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Main : MenuScreen
    {
        [SerializeField] private Button _startActiveGraphButton;

        public override void Show()
        {
            base.Show();

            if (_startActiveGraphButton != null)
            {
#if UNITY_EDITOR
                _startActiveGraphButton.gameObject.SetActive(true);
#else
                _startActiveGraphButton.gameObject.SetActive(false);
#endif
            }
        }

        public void OnStartPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.SELECT_SCENARIO);
        }

        public void OnLoadActiveGraphPressed()
        {
            if ((ScenarioLoader.Instance != null) && (ScenarioLoader.Instance.GetCurrentGraphConfig() != null))
            {
                MainMenuController?.RequestLoadSandboxScene(() => ScenarioLoader.Instance.RequestStartActiveGraph());
            }
            else
            {
                //TODO(caspar): Error
            }
        }
    }
}
