using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Select : MenuScreen
    {
        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.MAIN);
        }

        public void OnLoadOtherScenarioPressed()
        {
            MainMenuController?.SetScreen(MainMenuController.Screen.LOAD_SCENARIO);
        }
    }
}
