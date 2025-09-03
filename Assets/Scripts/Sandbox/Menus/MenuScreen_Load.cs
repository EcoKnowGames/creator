using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Load : MenuScreen
    {
        public void OnLoadFromFilePressed()
        {
            ScenarioLoader.ShowLoadDialog(OnScenarioLoaded, null);
        }

        public void OnLoadFromJSONPressed()
        {

        }

        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.SELECT_SCENARIO);
        }

        private void OnScenarioLoaded(Scenario scenario)
        {
            //TODO(caspar)
        }
    }
}
