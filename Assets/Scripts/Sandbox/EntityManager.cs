using System;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    [System.Serializable]
    public record Entity
        (
        string ID,
        string Icon,
        string Colour,
        float GrowthRate,
        float MovementRate,

        float VulnerableThreshold,
        float AbundanceThreshold,

        bool AutoPlace,
        int StartPopulation,

        bool CanHarvest,
        bool CanIntroduce,

        int HarvestLimit,
        int IntroduceLimit,

        Quantity[] HarvestQuantities,
        Quantity[] IntroduceQuantities,

        EntityZoneInformation[] ZoneInformation,

        // Player-visibility flags. Both default false so existing scenario JSONs (which omit
        // them) deserialise unchanged. Presentational only: hidden entities still simulate,
        // count towards win/lose conditions, and appear in the data export.
        bool HiddenFromCellToken = false,   // no per-cell grid token; still listed in the panel
        bool HiddenFromEntityPanel = false, // absent from the panel; grid tokens still render
        bool HiddenFromGraph = false        // absent from the population graph; still simulates
        );


    [System.Serializable]
    public class EntityZoneInformation
    {
        [SerializeField] public int ZoneID;
        [SerializeField] public float GrowthRate;
        [SerializeField] public float MovementRate;
        [SerializeField] public List<int> Transitions;

        public EntityZoneInformation(int zoneID) { ZoneID = zoneID; Transitions = new List<int>(); }
    }

    public delegate void EntityEvent(int column, int row, int id);

    //This class stores and handles manipulation of the Entity data
    //Data can be requested or modified here
    public class EntityManager : MonoBehaviour
    {
        private IEntityCalculator _entityCalculator = CalculatorRegistry.Create(CalculatorRegistry.DefaultId); // Default; overridden per scenario via RegisterCalculator
        public IEntityCalculator Calculator => _entityCalculator;

        private Matrix _entityMatrix;
        private Dictionary<int, float[,]> _zoneAlphaMatrices;

        public float[,] AlphaMatrix => _entityMatrix.entityMatrix;
        private Entity[] _entityTypeList;
        public int EntityTypeCount => _entityTypeList.Length;


        //Cell lookup table
        //X, Y, entityIndex
        private int[,,] _entityLookupTable;

        public EntityEvent onEntityHarvested;
        public EntityEvent onEntityIntroduced;

        private const string LogChannel = "[EntityManager]";

        #region Setup
        public void RegisterAlphaMatrix(Matrix matrix)
        {
            _entityMatrix = matrix;
        }

        public void RegisterCalculator(IEntityCalculator calculator)
        {
            if (calculator == null)
            {
                Debug.LogWarning("[EntityManager] RegisterCalculator called with null; keeping the current calculator.");
                return;
            }

            _entityCalculator = calculator;
        }

        public void RegisterEntities(List<Entity> entityRecords)
        {
            if ((entityRecords == null) || (entityRecords.Count <= 0) || _entityMatrix == null)
            {
                return;
            }

            //Make sure the order we register matches the order from the alpha matrix. This will be important for maths later
            _entityTypeList = entityRecords.OrderBy(x => Array.IndexOf(_entityMatrix.entityIDs, x.ID)).ToArray();
        }

        public void AddEntitiesToGrid(GridDef gridDef, GridManager gridManager)
        {
            if ((gridDef == null) || (gridManager == null))
            {
                return;
            }

            Vector2 gridSize = gridManager.GridSize;
            _entityLookupTable = new int[(int)gridSize.x, (int)gridSize.y, EntityTypeCount];

            //Add arbritrary amount of entities to each cell for now
            for (int row = 0; row < gridSize.y; row++)
            {
                for (int column = 0; column < gridSize.x; column++)
                {
                    for (int i = 0; i < EntityTypeCount; i++)
                    {
                        Entity entityType = GetEntityType(i);

                        int startPopulation = gridDef.HasPopulations() ? gridDef.GetPopulation(column, row, i) : entityType.AutoPlace == true ? entityType.StartPopulation : 0;
                        _entityLookupTable[column, row, i] = gridManager.FindCellAtPosition(column, row) != null ? startPopulation : -1;
                    }
                }
            }

            StartCoroutine(gridManager.OnEntitiesAdded());
        }
        #endregion

        #region Zones
        public void RegisterZoneAlphaMatrices(Matrix[] matrices)
        {
            if (matrices == null || _entityTypeList == null)
            {
                return;
            }

            _zoneAlphaMatrices = new Dictionary<int, float[,]>();

            // Also set _entityMatrix to the first matrix for legacy compat
            if (matrices.Length > 0)
            {
                _entityMatrix = matrices[0];
            }

            int entityCount = _entityTypeList.Length;
            string[] masterIDs = _entityTypeList.Select(x => x.ID).ToArray();

            foreach (Matrix m in matrices)
            {
                float[,] zoneMatrix = new float[entityCount, entityCount];

                if (m.entityIDs != null && m.entityMatrix != null)
                {
                    // Map each matrix entity ID to master index
                    int[] indexMap = new int[m.entityIDs.Length];
                    for (int i = 0; i < m.entityIDs.Length; i++)
                    {
                        indexMap[i] = Array.IndexOf(masterIDs, m.entityIDs[i]);
                    }

                    for (int row = 0; row < m.entityIDs.Length; row++)
                    {
                        for (int col = 0; col < m.entityIDs.Length; col++)
                        {
                            int masterRow = indexMap[row];
                            int masterCol = indexMap[col];
                            if (masterRow >= 0 && masterCol >= 0)
                            {
                                zoneMatrix[masterCol, masterRow] = m.entityMatrix[col, row];
                            }
                        }
                    }
                }

                _zoneAlphaMatrices[m.zoneIndex] = zoneMatrix;
            }
        }

        public float[,] GetAlphaMatrixForZone(int zoneID)
        {
            if (_zoneAlphaMatrices != null && _zoneAlphaMatrices.TryGetValue(zoneID, out float[,] matrix))
            {
                return matrix;
            }

            return _entityMatrix?.entityMatrix;
        }

        public int GetZoneType(int column, int row)
        {
            if (SandboxManager.Instance.GridManager == null)
            {
                return 0;
            }

            return SandboxManager.Instance.GridManager.GetZoneType(column, row);
        }
        #endregion

        #region Entity Data
        public Entity GetEntityType(int index)
        {
            if (index >= _entityTypeList.Count() || index < 0)
            {
                Debug.LogError($"{LogChannel} Failed to find Entity Type with index [{index}], index is invalid!");
                return null;
            }

            return _entityTypeList[index];
        }

        public Entity[] GetEntityTypeList()
        {
            if ((_entityTypeList == null) || (_entityTypeList.Length <= 0))
            {
                Debug.LogError($"{LogChannel} Failed to return Entity Type List, list is null or empty!");
                return null;
            }

            return _entityTypeList;
        }

        public int GetEntityIndex(Entity type)
        {
            return Array.IndexOf(_entityTypeList, type);
        }

        public int GetEntityIndex(string id)
        {
            return Array.IndexOf(_entityTypeList, _entityTypeList.FirstOrDefault(x => x.ID.Equals(id, StringComparison.OrdinalIgnoreCase)));
        }

        public CellEntity[] GetEntitiesForCell(int column, int row)
        {
            CellEntity[] entityCounts = new CellEntity[EntityTypeCount];

            for (int i = 0; i < EntityTypeCount; i++)
            {
                Entity type = _entityTypeList[i];

                string id = type.ID;
                int population = _entityLookupTable[column, row, i];

                CellEntity.State state = CellEntity.State.STABLE;
                if (population == 0)
                {
                    state = CellEntity.State.EXTINCT;
                }
                else if (population <= type.VulnerableThreshold)
                {
                    state = CellEntity.State.VULNERABLE;
                }
                else if (population >= type.AbundanceThreshold)
                {
                    state = CellEntity.State.ABUNDANT;
                }

                entityCounts[i] = new CellEntity(i, id, population, state);
            }

            return entityCounts;
        }

        public int GetPopulationInCell(int column, int row, int index)
        {
            if (index >= _entityLookupTable.GetLongLength(2) || index < 0)
            {
                Debug.LogError($"{LogChannel} Failed to find population of Entity with index [{index}] in Cell [{column}, {row}], index is invalid!");
                return 0;
            }

            return _entityLookupTable[column, row, index];
        }

        public Dictionary<string, int> GetPopulationsInCell(int column, int row)
        {
            Dictionary<string, int> populations = new Dictionary<string, int>();

            for (int i = 0; i < EntityTypeCount; i++)
            {
                Entity type = _entityTypeList[i];
                int population = _entityLookupTable[column, row, i];
                populations.Add(type.ID, population);
            }

            return populations;
        }

        public Dictionary<string, int>[,] GetPopulationsByCell()
        {
            Dictionary<string, int>[,] populations = new Dictionary<string, int>[_entityLookupTable.GetLongLength(0), _entityLookupTable.GetLongLength(1)];

            for (int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
            {
                for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                {
                    populations[column, row] = GetPopulationsInCell(column, row);
                }
            }

            return populations;
        }

        public int GetTotalPopulationOfEntityType(int index)
        {
            if (index >= _entityLookupTable.GetLongLength(2) || index < 0)
            {
                Debug.LogError($"{LogChannel} Failed to find total population of Entity with index [{index}], index is invalid!");
                return 0;
            }

            //Find total
            int totalPopulation = 0;
            for (int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
            {
                for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                {
                    int population = _entityLookupTable[column, row, index];
                    if (population > 0)
                    {
                        totalPopulation += population; //-1 population means the entity/cell is not valid, so don't add it
                    }
                }
            }

            return totalPopulation;
        }

        public int GetHarvestLimits(int index)
        {
            Entity type = GetEntityType(index);
            if (type != null)
            {
                return type.HarvestLimit;
            }

            return 0;
        }

        public int GetIntroduceLimits(int index)
        {
            Entity type = GetEntityType(index);
            if (type != null)
            {
                return type.IntroduceLimit;
            }

            return 0;
        }

        public Quantity[] GetHarvestRewards(int index)
        {
            Entity type = GetEntityType(index);
            if (type != null)
            {
                return type.HarvestQuantities;
            }

            return null;
        }

        public Quantity[] GetIntroduceCosts(int index)
        {
            Entity type = GetEntityType(index);
            if (type != null)
            {
                return type.IntroduceQuantities;
            }

            return null;
        }
        #endregion

        #region Data Manipulation
        public bool TryHarvestEntityFromCell(int column, int row, int index, int amount)
        {
            if (column < 0 || row < 0 || column >= _entityLookupTable.GetLongLength(0) || row >= _entityLookupTable.GetLongLength(1))
            {
                Debug.LogError($"{LogChannel} Failed to harvest entity from Cell [{column}, {row}], location out of bounds!");
                return false;
            }

            if (index >= 0 && index < _entityLookupTable.GetLongLength(2))
            {
                //Check our type first
                Entity type = _entityTypeList[index];
                if (type == null || !type.CanHarvest)
                {
                    Debug.LogError($"{LogChannel} Failed to harvest entity from Cell [{column}, {row}]. Entity index {index} cannot be harvested!");
                    return false;
                }

                //Check population
                int currentPopulation = _entityLookupTable[column, row, index];
                if (currentPopulation == 0) //Fail interaction if we have nothing to harvest
                {
                    return false;
                }

                //TODO: We cannot harvest more than we have in the cell, so what sort of user feedback should we get if we try to harvest too much?

                int newPopulation = Mathf.Max(currentPopulation - amount, 0);
                int difference = newPopulation - currentPopulation;
                _entityLookupTable[column, row, index] = newPopulation;

                SandboxManager.Instance.PlayerInventory.AddQuantities(type.HarvestQuantities, Math.Abs(difference));
                Debug.Log($"{LogChannel} [HARVEST Entity {index}] Current: {currentPopulation} / New: {newPopulation} / Difference: {difference}");

                onEntityHarvested?.Invoke(column, row, index);

                return true;
            }
            else
            {
                Debug.LogError($"{LogChannel} Failed to harvest entity from Cell [{column}, {row}]. Entity index {index} is invalid!");
            }

            return false;
        }

        public bool TryIntroduceEntityToCell(int column, int row, int index, int amount)
        {
            if (column < 0 || row < 0 || column >= _entityLookupTable.GetLongLength(0) || row >= _entityLookupTable.GetLongLength(1))
            {
                Debug.LogError($"{LogChannel} Failed to introduce entity to Cell [{column}, {row}], location out of bounds!");
                return false;
            }

            if (index >= 0 && index < _entityLookupTable.GetLongLength(2))
            {
                //Check our type first
                Entity type = _entityTypeList[index];
                if (type == null || !type.CanIntroduce)
                {
                    Debug.LogError($"{LogChannel} Failed to introduce entity to Cell [{column}, {row}]. Entity index {index} cannot be introduced!");
                    return false;
                }

                bool hasRequiredQuantities = SandboxManager.Instance.PlayerInventory.HasQuantities(type.IntroduceQuantities, amount);
                if (!hasRequiredQuantities)
                {
                    Debug.LogError($"{LogChannel} Failed to introduce entity to Cell [{column}, {row}]. Player does not have the required resources to introduce Entity of type {index}");
                    return false;
                }

                int currentPopulation = _entityLookupTable[column, row, index];
                int newPopulation = currentPopulation + amount;
                int difference = newPopulation - currentPopulation;
                _entityLookupTable[column, row, index] = newPopulation;

                SandboxManager.Instance.PlayerInventory.RemoveQuantities(type.IntroduceQuantities, amount);
                Debug.Log($"{LogChannel} [INTRODUCE Entity {index}] Current: {currentPopulation} / New: {newPopulation} / Difference: {difference}");

                onEntityIntroduced?.Invoke(column, row, index);

                return true;
            }
            else
            {
                Debug.LogError($"{LogChannel} Failed to introduce entity to Cell [{column}, {row}]. Entity index {index} is invalid!");
            }

            return false;
        }
        #endregion


        #region Population and Movement Maths
        public void PerformCalculations()
        {
            _entityCalculator?.CalculatePopulations(this, _entityLookupTable);
            _entityCalculator?.CalculateMovement(this, _entityLookupTable);
        }
 
        public int GetValidNeighbourCount(int column, int row, int entity)
        {
            int neighbours = 0;
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        int xPos = column + x;
                        int yPos = row + y;

                        if (xPos >= 0 &&
                            xPos < _entityLookupTable.GetLongLength(0) &&
                            yPos >= 0 &&
                            yPos < _entityLookupTable.GetLongLength(1))
                        {
                            //Populations less than 0 are invalid cells
                            int population = _entityLookupTable[xPos, yPos, entity];
                            if (population >= 0)
                            {
                                neighbours += 1;
                            }
                        }
                    }
                }
            }

            return neighbours;
        }

        public Vector2[] FindOppositeEdges(int column, int row, int entity, bool ignoreNeighbours = false)
        {
            List<Vector2> oppositeEdges = new List<Vector2>();

            //Test Left
            bool edgeLeft = true;
            bool edgeRight = true;

            for (int i = 0; i < _entityLookupTable.GetLongLength(0); i++)
            {
                if (_entityLookupTable[i, row, entity] >= 0)
                {
                    if (i < column)
                    {
                        edgeLeft = false;
                    }
                    else if (i > column)
                    {
                        edgeRight = false;
                    }
                }
            }

            bool edgeTop = true;
            bool edgeBottom = true;
            for (int j = 0; j < _entityLookupTable.GetLongLength(1); j++)
            {
                if (_entityLookupTable[column, j, entity] >= 0)
                {
                    if (j < row)
                    {
                        edgeTop = false;
                    }
                    else if (j > row)
                    {
                        edgeBottom = false;
                    }
                }
            }


            //Now gather our opposite Cells if relevant
            if (edgeLeft)
            {
                int limit = ignoreNeighbours ? column + 1 : column;
                for (int i = (int)_entityLookupTable.GetLongLength(0) - 1; i > limit; i--)
                {
                    if (_entityLookupTable[i, row, entity] >= 0)
                    { 
                        oppositeEdges.Add(new Vector2(i, row));
                        break;
                    }
                }
            }

            if (edgeRight)
            {
                int limit = ignoreNeighbours ? column - 1 : column;
                for (int j = 0; j < limit; j++)
                {
                    if (_entityLookupTable[j, row, entity] >= 0)
                    {
                        oppositeEdges.Add(new Vector2(j, row));
                        break;
                    }
                }
            }

            if (edgeTop)
            {
                int limit = ignoreNeighbours ? row + 1 : row;
                for (int k = (int)_entityLookupTable.GetLongLength(1) - 1; k > limit; k--)
                {
                    if (_entityLookupTable[column, k, entity] >= 0)
                    {
                        oppositeEdges.Add(new Vector2(column, k));
                        break;
                    }
                }
            }

            if (edgeBottom)
            {
                int limit = ignoreNeighbours ? row - 1 : row;
                for (int l = 0; l < limit; l++)
                {
                    if (_entityLookupTable[column, l, entity] >= 0)
                    {
                        oppositeEdges.Add(new Vector2(column, l));
                        break;
                    }
                }
            }

            return oppositeEdges.ToArray();
        }
        #endregion
    }
}
