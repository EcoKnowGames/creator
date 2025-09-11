using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{

    public class PlayerDef
    {
        private string _nameOverride;
        public string Name => _nameOverride;

        public void SetName(string name)
        {
            _nameOverride = name;
        }
    }

    public class MultiplayerManager : MonoBehaviour
    {
        #region Singleton
        protected static MultiplayerManager instance;
        public static MultiplayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetInstance();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(MultiplayerManager) +
                            " is needed in the scene, but there is none.");
                    }
                }

                return instance;
            }
        }

        public static bool Exists
        {
            get
            {
                return instance;
            }
        }

        private static MultiplayerManager GetInstance()
        {
            if (instance == null)
            {
                return FindFirstObjectByType<MultiplayerManager>();
            }
            return instance;
        }
        #endregion


        [SerializeField] private ColourPaletteObject _multiplayerColourPalette;
        private PlayerDef[] _playerDefs = null;

        [SerializeField] private int _maxPlayers = 4;
        public int MaxPlayers => _maxPlayers;

        private int _currentPlayerIndex = -1;
        public int CurrentPlayerIndex => _currentPlayerIndex;
        public bool IsMultiplayer => _maxPlayers > 1;

        private const string LogChannel = "[ScenarioLoader]";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SetMaxPlayers(1); //Default
        }

        public void SetMaxPlayers(int max)
        {
            max = Mathf.Clamp(max, 1, 4);
            _maxPlayers = max;

            _playerDefs = new PlayerDef[_maxPlayers];
            for (int i = 0; i < _playerDefs.Length; i++)
            {
                _playerDefs[i] = new PlayerDef();
            }
        }

        public void SetCurrentPlayer(int index)
        {
            if (index >= _maxPlayers)
            {
                Debug.LogError($"{LogChannel} Failed to set current player to {index}, max players is {_maxPlayers}");
                index = Mathf.Clamp(index, 0, _maxPlayers);
            }

            _currentPlayerIndex = index;
        }

        public PlayerDef GetPlayerDef(int index)
        {
            if ((index < 0) || (index >= _maxPlayers))
            {
                Debug.LogError($"{LogChannel} Failed to get PlayerDef for index {index}, index is invalid");
                return null;
            }

            if (_playerDefs == null)
            {
                Debug.LogError($"{LogChannel} Failed to get PlayerDef for index {index}, PlayerDefs is null or invalid");
                return null;

            }

            return _playerDefs[index];
        }

        public string GetPlayerName(int index)
        {
            PlayerDef playerDef = GetPlayerDef(index);
            if (playerDef != null)
            {
                if (!string.IsNullOrEmpty(playerDef.Name))
                {
                    return playerDef.Name;
                }
            }

            return $"Player {index + 1}"; //Account for 0
        }

        public Color GetPlayerColour(int index)
        {
            if ((index < 0) || (index >= _maxPlayers))
            {
                Debug.LogError($"{LogChannel} Failed to get PlayerDef for index {index}, index is invalid");
                return Color.white;
            }

            if ((_multiplayerColourPalette == null) || (_multiplayerColourPalette.Colours.Length < index))
            {
                Debug.LogError($"{LogChannel} Failed to get Player Colour for index {index}, Multiplayer Colour Palette is null or invalid");
                return Color.white;
            }

            return _multiplayerColourPalette.Colours[index].colour;
        }

        public void RenamePlayer(int index, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"{LogChannel} Failed to rename player {index}, the provided name string is empty or null. Resetting to default name");
                //return;
            }

            PlayerDef playerDef = GetPlayerDef(index);
            if (playerDef != null)
            {
                playerDef.SetName(name);
            }
        }
    }
}
