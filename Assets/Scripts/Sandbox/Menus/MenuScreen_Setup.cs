using System.Linq;
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

        [SerializeField] private Transform _playerButtonContainer;
        private Button_PlayerSelect[] _playerButtonList => _playerButtonContainer == null ? null : _playerButtonContainer.GetComponentsInChildren<Button_PlayerSelect>().OrderBy(x => x.transform.GetSiblingIndex()).ToArray(); //Just in case this for some reason does not return child order

        public override void Show()
        {
            base.Show();

            MultiplayerManager.Instance?.SetMaxPlayers(1); //Singleplayer is default
            AddListeners();
            RefreshButtons();

            if (_playerButtonList != null)
            {
                for(int i = 0; i < _playerButtonList.Length; i++)
                {
                    _playerButtonList[i].ResetNameInput(i);
                    _playerButtonList[i].SetPlayerCount(i);
                }
            }
        }

        public override void Hide()
        {
            base.Hide();
            RemoveListeners();
        }

        private void AddListeners()
        {
            if (_playerButtonList != null)
            {
                for (int i = 0; i < _playerButtonList.Length; i++)
                {
                    Button_PlayerSelect playerButton = _playerButtonList[i];
                    int playerIndex = i;
                    playerButton?.PlayerNameButton?.onClick.AddListener(() => {
                        MultiplayerManager.Instance.RenamePlayer(playerIndex, playerButton.PlayerName);
                        playerButton.OnNameSet();
                        });
                }
            }
        }

        private void RemoveListeners()
        {

            if (_playerButtonList != null)
            {
                for (int i = 0; i < _playerButtonList.Length; i++)
                {
                    _playerButtonList[i].PlayerNameButton?.onClick.RemoveAllListeners();
                }
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
            if (_playerButtonList != null)
            {
                for (int i = 0; i < _playerButtonList.Length; i++)
                {
                    _playerButtonList[i].SetSelected(maxPlayers == (i + 1));
                }
            }

            if (_playerButtonList != null)
            {
                for (int i = 0; i < _playerButtonList.Length; i++)
                {
                    if (i == 0)
                    {
                        _playerButtonList[i].SetPlayerFieldInteractable(true);
                    }
                    else
                    {
                        _playerButtonList[i].SetPlayerFieldInteractable((i + 1) <= maxPlayers);
                    }
                }
            }
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

            if (_playerButtonList != null)
            {
                for (int i = 0; i < _playerButtonList.Length; i++)
                {
                    if (i == 0)
                    {
                        _playerButtonList[i].SetInteractable(true);
                    }
                    else
                    {
                        _playerButtonList[i].SetInteractable(maxActions % (i + 1) == 0);
                    }
                }
            }
        }
    }
}
