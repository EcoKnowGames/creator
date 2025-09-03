using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox.Menus
{
    public class MenuScreen_Setup : MenuScreen
    {
        private MainMenuController.Screen _entryScreen = MainMenuController.Screen.SELECT_SCENARIO;

        [Header("Buttons")]
        [SerializeField] private Button_PlayerSelect _playerButton_One;
        [SerializeField] private Button_PlayerSelect _playerButton_Two;
        [SerializeField] private Button_PlayerSelect _playerButton_Three;
        [SerializeField] private Button_PlayerSelect _playerButton_Four;

        public override void Show()
        {
            base.Show();

            MultiplayerManager.Instance?.SetMaxPlayers(1); //Singleplayer is default
            AddListeners();
            RefreshButtons();
        }

        public override void Hide()
        {
            base.Hide();
            RemoveListeners();
        }

        private void AddListeners()
        {
            _playerButton_One?.PlayerNameButton?.onClick.AddListener(() => MultiplayerManager.Instance.RenamePlayer(0, _playerButton_One.PlayerName));
            _playerButton_Two?.PlayerNameButton?.onClick.AddListener(() => MultiplayerManager.Instance.RenamePlayer(1, _playerButton_Two.PlayerName));
            _playerButton_Three?.PlayerNameButton?.onClick.AddListener(() => MultiplayerManager.Instance.RenamePlayer(2, _playerButton_Three.PlayerName));
            _playerButton_Four?.PlayerNameButton?.onClick.AddListener(() => MultiplayerManager.Instance.RenamePlayer(3, _playerButton_Four.PlayerName));
        }

        private void RemoveListeners()
        {
            _playerButton_One?.PlayerNameButton?.onClick.RemoveAllListeners();
            _playerButton_Two?.PlayerNameButton?.onClick.RemoveAllListeners();
            _playerButton_Three?.PlayerNameButton?.onClick.RemoveAllListeners();
            _playerButton_Four?.PlayerNameButton?.onClick.RemoveAllListeners();
        }

        public void SetEntryScreen(MainMenuController.Screen entryScreen)
        {
            _entryScreen = entryScreen;
        }

        public void OnReturnPressed()
        {
            MainMenuController?.SetScreen(_entryScreen);
        }

        public void OnStartGamePressed()
        {
            MainMenuController.RequestLoadSandboxScene(() => ScenarioLoader.Instance.RequestStartLoadedConfig());
        }

        public void SetMaxPlayers(int maxPlayers)
        {
            MultiplayerManager.Instance?.SetMaxPlayers(maxPlayers);
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            SetSelectedPlayerButtons();
            SetValidPlayerButtons();
        }

        private void SetSelectedPlayerButtons()
        {
            //Setup Selected
            int maxPlayers = MultiplayerManager.Instance.MaxPlayers;
            _playerButton_One?.SetSelected(maxPlayers == 1);
            _playerButton_Two?.SetSelected(maxPlayers == 2);
            _playerButton_Three?.SetSelected(maxPlayers == 3);
            _playerButton_Four?.SetSelected(maxPlayers == 4);

            _playerButton_One?.SetPlayerFieldInteractable(true);
            _playerButton_Two?.SetPlayerFieldInteractable(2 <= maxPlayers);
            _playerButton_Three?.SetPlayerFieldInteractable(3 <= maxPlayers);
            _playerButton_Four?.SetPlayerFieldInteractable(4 <= maxPlayers);
        }

        private void SetValidPlayerButtons()
        {
            //Setup Interactable
            int maxActions = 1;
            ScenarioConfig config = ScenarioLoader.Instance.LoadedConfig;
            if (config != null)
            {
                maxActions = config.Scenario.ActionsPerRound;
            }

            _playerButton_One?.SetInteractable(true);
            _playerButton_Two?.SetInteractable(maxActions % 2 == 0);
            _playerButton_Three?.SetInteractable(maxActions % 3 == 0);
            _playerButton_Four?.SetInteractable(maxActions % 4 == 0);
        }
    }
}
