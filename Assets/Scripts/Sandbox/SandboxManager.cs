using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox.Data;
using Glitchers.EcoKnow.Sandbox.Grid;
using Glitchers.EcoKnow.Sandbox.UI;
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
            ScenarioLoader.ShowLoadDialog(StartNewGame, null);
        }

        void Update()
        {
            _gridManager?.HandleInput();
        }

        public void StartNewGame(Scenario scenario)
        {
            if (scenario != null)
            {
                Debug.Log($"Scenario Name is: {scenario.Name}");
                Debug.Log($"Map Layout is: {scenario.Map.fileName}");

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
                _sandboxUI?.Init(_entityManager.GetEntityTypeList());

                StartNewRound();
            }
        }

        public void ReplayCurrentScenario()
        {
            if (ScenarioLoader.Instance.LastPlayedScenario != null)
            {
                StartNewGame(ScenarioLoader.Instance.LastPlayedScenario);
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
            //Check our win conditions
            foreach (WinCondition winCondition in _winConditions)
            {
                winCondition.OnNewRound();
            }

            //Now track data
            Data.DataManager.Instance.RecordEvent(Data.EventType.ROUND_END);

            //Branch based on current round number
            bool finalRound = _currentRound >= _maxRounds - 1;
            if (finalRound)
            {
                EndGame();
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

            if (_currentRound == 0)
            {
                //Capture here to make sure we have our starting action count and Player index
                DataManager.Instance.RecordEvent(Data.EventType.GAME_START);
            }
        }

        private void EndGame()
        {
            bool playerWins = AreWinConditionsMet();
            _sandboxUI?.OnGameEnded(playerWins == true ? Result.WIN : Result.LOSE);
            Data.DataManager.Instance.RecordEvent(Data.EventType.GAME_END);

        }
        #endregion

        #region WinConditions
        private bool AreWinConditionsMet()
        {
            //Check win conditions first
            int totalWinConditionsCompleted = 0;
            foreach (WinCondition winCondition in _winConditions)
            {
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

        #region Actions
        public static bool CanPerformAction()
        {
            if ((Instance != null) && (Instance.PlayerInventory != null))
            {
                return Instance.PlayerInventory.GetAmountHeld(PlayerInventory.ActionID) > 0;
            }

            return false;
        }

        public static int SpendActionPoint()
        {
            int actionsRemaining = 0;
            if ((Instance != null) && (Instance.PlayerInventory != null))
            {
                actionsRemaining = Instance.PlayerInventory.RemoveItem(PlayerInventory.ActionID, 1);
            }

            return actionsRemaining;
        }

        public static int GetAvailableActionPoints()
        {
            int actionsRemaining = 0;
            if ((Instance != null) && (Instance.PlayerInventory != null))
            {
                actionsRemaining = Instance.PlayerInventory.GetAmountHeld(PlayerInventory.ActionID);
            }

            return actionsRemaining;
        }
        #endregion
    }
}
