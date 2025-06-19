using System.Collections.Generic;
using System.Linq;
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

        [Header("Scenario")]
        [SerializeField] private ScenarioNodeGraph _scenarioNodeGraph;

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

        public enum Result { WIN, LOSE };
        private List<WinCondition> _winConditions;
        public List<WinCondition> WinConditions => _winConditions;

        #region Lifecycle
        void Start()
        {
            StartNewGame();
        }

        void Update()
        {
            _gridManager?.HandleInput();
        }

        public void StartNewGame()
        {
            if (_scenarioNodeGraph != null)
            {
                ScenarioNode scenarioNode = _scenarioNodeGraph.GetScenarioNode();
                Debug.Log($"Scenario Name is: {scenarioNode.Name}");
                Debug.Log($"Map Layout is: {scenarioNode.GetMapLayout().fileName}");

                //Setup Random
                Random.InitState(scenarioNode.Seed);

                //Setup rounds
                _currentRound = -1;
                _maxRounds = scenarioNode.TotalRounds;
                _maxActionsPerRound = scenarioNode.ActionsPerRound;

                InitWinConditions(_scenarioNodeGraph.GetWinConditionList());

                //Init grid
                _gridManager?.Init();

                //Init inventory
                _playerInventory?.Init();
                _playerInventory?.AddItem(PlayerInventory.CurrencyID, scenarioNode.StartCurrency);

                //Setup entities
                MatrixNode matrixNode = _scenarioNodeGraph.GetMatrixNode();
                if (matrixNode != null)
                {
                    _entityManager.RegisterAlphaMatrix(matrixNode.matrix);
                }

                if (_scenarioNodeGraph.HasConnectedEntityNodes())
                {
                    _entityManager.RegisterEntities(_scenarioNodeGraph.GetEntityList());
                }

                if (_scenarioNodeGraph.HasConnectedItemNodes())
                {
                    _playerInventory.RegisterItemDefinitions(_scenarioNodeGraph.GetItemList());
                }

                //Setup grid
                GridDef gridDef = scenarioNode.GetMapLayout().gridDef;
                _gridManager?.EnableGrid();
                _gridManager?.SetupGrid(gridDef);

                _entityManager.AddEntitiesToGrid(_gridManager);

                //Set up all of our UI
                _sandboxUI?.Init();
            }

            StartNewRound();
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

            foreach(WinConditionRecord record in winConditions)
            {
                WinCondition condition = new WinCondition();
                condition.Init(record);

                _winConditions.Add(condition);
            }
        }
        #endregion
    }
}
