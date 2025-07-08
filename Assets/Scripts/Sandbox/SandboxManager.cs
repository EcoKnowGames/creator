using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox.Grid;
using Glitchers.EcoKnow.Sandbox.UI;
using Newtonsoft.Json;
using SimpleFileBrowser;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public class SandboxManager : MonoBehaviour
    {
        #region Singleton
        protected static SandboxManager instance;
        public static SandboxManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetInstance();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(SandboxManager) +
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

        private static SandboxManager GetInstance()
        {
            if (instance == null)
            {
                return FindFirstObjectByType<SandboxManager>();
            }
            return instance;
        }
        #endregion

        //[Header("Test")]
        //[SerializeField, TextArea] private string _scenarioJson;

        [Header("Scenario")]
        [SerializeField] private ScenarioNodeGraph _scenarioNodeGraph;
        private Scenario _lastPlayedScenario;

        [Header("Gameplay")]
        [SerializeField] private EntityManager _entityManager;
        public EntityManager EntityManager => _entityManager;
        [SerializeField] private GridManager _gridManager;
        public GridManager GridManager => _gridManager;

        [SerializeField] private PlayerInventory _playerInventory;
        public PlayerInventory PlayerInventory => _playerInventory;

        [Header("UI")]
        [SerializeField] private SandboxUI _sandboxUI;


        //TODO(caspar): Should rounds and win conditions be handled in another location?
        private int _maxRounds = 1;
        private int _currentRound = -1;

        private int _maxActionsPerRound = 1;

        private int _defaultRounds = 2;
        private int _defaultActionsPerRound = 2;

        public enum Result { WIN, LOSE };
        private List<WinCondition> _winConditions;
        public List<WinCondition> WinConditions => _winConditions;

        private const string LogChannel = "[SandboxManager]";

        #region Lifecycle
        void Start()
        {
            ShowLoadDialog();
        }

        void Update()
        {
            _gridManager?.HandleInput();
        }

        //TODO(caspar): Probably move all of this file loading stuff to a main menu/bootstrap script. We should only need to pass the Sandbox the parsed ScenarioConfig and nothing else!
        private void ShowLoadDialog()
        {
            FileBrowser.SetDefaultFilter(".json");
            FileBrowser.ShowLoadDialog(
            (filePaths) =>
            {
                //On file chosen
                string rawJson = ParseJsonFromFile(filePaths);
                if (!string.IsNullOrEmpty(rawJson))
                {
                    ScenarioConfig config = LoadConfig(rawJson);
                    if (config != null)
                    {
                        StartNewGame(config.Scenario);
                    }
                }
            },
            () =>
            {
                //On cancelled
                Debug.LogError($"{LogChannel} Failed to load a valid JSON file");
            },
            FileBrowser.PickMode.Files,
            false,
            null,
            null,
            "Load Scenario JSON File"
            );
        }

        private string ParseJsonFromFile(string[] filePaths)
        {
            if ((filePaths != null) && (filePaths.Length > 0))
            {
                //Ignore multi-select
                string filePath = filePaths[0];

                //Read from file
                string text = System.IO.File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            Debug.LogError($"{LogChannel} Failed to parse Json, either filePath was invalid or the selected file was empty");
            return null;
        }

        //Load ScenarioConfig from raw json
        private ScenarioConfig LoadConfig(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON, JSON is null or empty");
                return null;
            }

            //Deserialise from JSON
            List<string> errors = new List<string>();
            ScenarioConfig config = JsonConvert.DeserializeObject<ScenarioConfig>(json,
                new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                });

            //Check and print errors, if any
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.Log($"{LogChannel} Failed to deserialise ScenarioConfig from JSON: {error}");
                }
            }

            if (config != null)
            {
                //Validation
                if (Application.version != config.AppVersion)
                {
                    Debug.LogWarning($"{LogChannel} Scenario {config.Scenario.Name} was exported from a different version of the Application {config.AppVersion} (Current: {Application.version}). Scenario may not work as intended");
                }
#if UNITY_EDITOR
                if (Application.unityVersion != config.UnityVersion)
                {
                    Debug.LogWarning($"{LogChannel} Scenario {config.Scenario.Name} was exported from a different version of the Unity Editor {config.UnityVersion} (Current: {Application.unityVersion}). Scenario may not work as intended");
                }
#endif

                if (config.Scenario == null)
                {
                    Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Scenario is null!");
                    return null;
                }
                else
                {
                    if (config.Scenario.Map == null)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Map Layout is null! Scenario must have a valid Map Layout");
                        return null;
                    }

                    if (config.Scenario.Matrix == null)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Matrix is null! Scenario must have a valid Matrix");
                        return null;
                    }

                    if (config.Scenario.Entities == null || config.Scenario.Entities.Count() <= 0)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. No Entities found in the JSON file. Scenario must have valid Entities");
                        return null;
                    }

                    if (config.Scenario.WinConditions == null || config.Scenario.WinConditions.Count() <= 0)
                    {
                        Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. No Win Conditions found in the JSON file. Scenario must have at least one valid Win Condition");
                        return null;
                    }

                    if (config.Scenario.Rounds <= 0)
                    {
                        Debug.LogWarning($"{LogChannel} Invalid number of rounds ({config.Scenario.Rounds}) found in Scenario {config.Scenario.Name}. Defaulting to {_defaultRounds} Rounds");
                    }

                    if (config.Scenario.ActionsPerRound <= 0)
                    {
                        Debug.LogWarning($"{LogChannel} Invalid number of actions per round ({config.Scenario.Rounds}) found in Scenario {config.Scenario.Name}. Defaulting to {_defaultActionsPerRound} Action");
                    }
                }

                //If we have reached this point we are valid
            }
            else
            {
                Debug.LogError($"{LogChannel} Failed to load ScenarioConfig from JSON. Likely an error with parsing/deserialisation");
            }

            return config;
        }

        public void StartNewGame(Scenario scenario)
        {
            if (scenario != null)
            {
                Debug.Log($"Scenario Name is: {scenario.Name}");
                Debug.Log($"Map Layout is: {scenario.Map.fileName}");

                _lastPlayedScenario = scenario;

                //Setup Random
                Random.InitState(scenario.Seed);

                //Setup rounds
                _currentRound = -1;
                _maxRounds = scenario.Rounds <= 0 ? _defaultRounds : scenario.Rounds;
                _maxActionsPerRound = scenario.ActionsPerRound <= 0 ? _defaultActionsPerRound : scenario.ActionsPerRound;

                InitWinConditions(scenario.WinConditions.ToList());

                //Init grid
                _gridManager?.Init();

                //Init inventory
                _playerInventory?.Init();
                _playerInventory?.AddItem(PlayerInventory.CurrencyID, scenario.StartCurrency);

                //Setup entities and items
                _entityManager.RegisterAlphaMatrix(scenario.Matrix);
                _entityManager.RegisterEntities(scenario.Entities.ToList());
                _playerInventory.RegisterItemDefinitions(scenario.Items.ToList());


                //Setup grid
                GridDef gridDef = scenario.Map.gridDef;
                _gridManager?.EnableGrid();
                _gridManager?.SetupGrid(gridDef);

                _entityManager.AddEntitiesToGrid(_gridManager);

                //Set up all of our UI
                _sandboxUI?.Init();

                StartNewRound();
            }
        }

        public void ReplayCurrentScenario()
        {
            if (_lastPlayedScenario != null)
            {
                StartNewGame(_lastPlayedScenario);
            }
        }
        #endregion

        #region Rounds and Steps
        public void OnAdvanceRoundPressed()
        {
            CalculateMaths();
            OnRoundEnded();
        }

        private void CalculateMaths()
        {
            _entityManager?.CalculateNewEntityCount();
            _entityManager?.CalculateMovement();
            _gridManager?.UpdateAllCells();
        }

        private void OnRoundEnded()
        {
            bool playerWins = AreWinConditionsMet();
            bool finalRound = _currentRound >= _maxRounds - 1;

            if (finalRound)
            {
                _sandboxUI?.OnGameEnded(playerWins == true ? Result.WIN : Result.LOSE);
            }
            else
            {
                StartNewRound();
            }
        }

        public void StartNewRound()
        {
            _currentRound += 1;

            //Update actions
            int actionsHeld = 0;
            if (_playerInventory != null)
            {
                actionsHeld = _playerInventory.GetAmountHeld(PlayerInventory.ActionID);
                int actionsToAdd = _maxActionsPerRound - actionsHeld;
                actionsHeld = _playerInventory.AddItem(PlayerInventory.ActionID, actionsToAdd);
            }

            //Update UI
            _sandboxUI?.OnNewRoundStarted(_currentRound, _maxRounds, actionsHeld);
        }
        #endregion

        #region WinConditions
        private bool AreWinConditionsMet()
        {
            //Check win conditions first
            int totalWinConditionsCompleted = 0;
            foreach (WinCondition winCondition in _winConditions)
            {
                winCondition.OnNewRound();
                totalWinConditionsCompleted += winCondition.Completed == true ? 1 : 0;
            }

            Debug.Log($"Win Conditions Met: {totalWinConditionsCompleted}");

            if (totalWinConditionsCompleted >= _winConditions.Count)
            {
                Debug.Log("WIN THE GAME!");
                return true;
            }

            return false;
        }

        private void InitWinConditions(List<WinConditionRecord> winConditions)
        {
            _winConditions = new List<WinCondition>();

            foreach (WinConditionRecord record in winConditions)
            {
                WinCondition condition = new WinCondition();
                condition.Init(record);

                _winConditions.Add(condition);
            }
        }
        #endregion
    }
}
