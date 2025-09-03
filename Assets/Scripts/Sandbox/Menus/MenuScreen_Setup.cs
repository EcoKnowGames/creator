using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Setup : MenuScreen
    {
        private MainMenuController.Screen _entryScreen = MainMenuController.Screen.SELECT_SCENARIO;

        [Header("Buttons")]
        [SerializeField] private GameObject _playerButton_One;
        [SerializeField] private GameObject _playerButton_Two;
        [SerializeField] private GameObject _playerButton_Three;
        [SerializeField] private GameObject _playerButton_Four;

        public override void Show()
        {
            base.Show();

            ScenarioConfig config = ScenarioLoader.Instance.LoadedConfig;
            if (config != null)
            {
                //int ac
            }
        }

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
