using System.Linq;
using Glitchers.EcoKnow.Sandbox.Grid;
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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gridManager.Init();

            if (_scenarioNodeGraph != null)
            {
                ScenarioNode scenarioNode = _scenarioNodeGraph.GetScenarioNode();
                Debug.Log($"Scenario Name is: {scenarioNode.Name}");
                Debug.Log($"Map Layout is: {scenarioNode.GetMapLayout().fileName}");

                MatrixNode matrixNode = _scenarioNodeGraph.GetMatrixNode();
                if (matrixNode != null)
                {
                    _entityManager.RegisterAlphaMatrix(matrixNode.matrix);
                }

                if (_scenarioNodeGraph.HasEntityNodes())
                {
                    _entityManager.RegisterEntities(_scenarioNodeGraph.nodes.OfType<EntityNode>().Select(x => x.GetEntity()).ToList());
                }

                GridDef gridDef = scenarioNode.GetMapLayout().gridDef;
                _gridManager?.EnableGrid();
                _gridManager?.SetupGrid(gridDef);
                _gridManager?.AddEntities(_entityManager.AllEntities);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void OnAdvanceMathsPressed()
        {
            CalculateMaths();
        }

        private void CalculateMaths()
        {
            if (_gridManager != null)
            {
                _gridManager.AdvanceRound();
            }
        }
    }
}
