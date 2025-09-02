using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Setup : MenuScreen
    {
        private MainMenuController.Screen _entryScreen = MainMenuController.Screen.SELECT_SCENARIO;

        public void SetEntryScreen(MainMenuController.Screen entryScreen)
        {
            _entryScreen = entryScreen;
        }

        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(_entryScreen);
        }
    }
}
