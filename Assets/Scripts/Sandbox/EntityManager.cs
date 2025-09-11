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

        Quantity[] HarvestQuantities,
        Quantity[] IntroduceQuantities
        );

    public delegate void EntityEvent(int column, int row, int id);

    //Note(caspar) -> This class stores and handles manipulation of the Entity data
    //Data can be requested or modified here
    public class EntityManager : MonoBehaviour
    {
        //private float[,] _alphaMatrix;
        private Matrix _entityMatrix;
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
        public void CalculateNewEntityCount()
        {
            for (int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
            {
                for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                {
                    CellEntity[] entityList = GetEntitiesForCell(column, row);

                    if ((entityList == null) || (entityList.Length <= 0))
                    {
                        Debug.LogError($"{LogChannel} No entities found for Cell [{row} , {column}]. Aborting calculations...");
                        return;
                    }

                    if (entityList.Length != EntityTypeCount)
                    {
                        Debug.LogError($"{LogChannel} Entity count [{entityList.Length}] for Cell [{row} , {column}] does not match the Simulation Entity count [{EntityTypeCount}]! Aborting calculations...");
                        return;
                    }

                    //Check for empty/invalid cells
                    int nullCount = 0;
                    foreach (CellEntity entity in entityList)
                    {
                        if (entity.Population < 0)
                        {
                            nullCount += 1;
                        }
                    }

                    if (nullCount >= EntityTypeCount)
                    {
                        //This cell is completely empty, don't bother calculating
                        //Debug.Log($"Row {row} / Column {column} is empty!");
                        continue;
                    }

                    float[,] A = AlphaMatrix;
                    if ((A.GetLongLength(0) != EntityTypeCount) || (A.GetLongLength(1) != EntityTypeCount))
                    {
                        Debug.LogError($"{LogChannel} Entity count [{entityList.Length}] does not match the entity count of the Alpha Matrix. Aborting calculations...");
                        return;
                    }

                    float[] r = _entityTypeList.Select(x => x.GrowthRate).ToArray();
                    float[] N = entityList.Select(x => (float)x.Population).ToArray();

                    //AN
                    float[] AN = new float[EntityTypeCount];
                    for (int yy = 0; yy < A.GetLongLength(1); yy++)
                    {
                        float result = 0f;
                        for (int xx = 0; xx < A.GetLongLength(0); xx++)
                        {
                            result += A[xx, yy] * N[xx];
                            //Debug.Log(A[xx, yy]);
                        }

                        AN[yy] = result;
                    }

                    //N + N.(r + AN)
                    float[] NNrAN = new float[EntityTypeCount];
                    for (int a = 0; a < N.Length; a++)
                    {
                        NNrAN[a] = N[a] + (N[a] * (r[a] + AN[a]));
                        //Debug.Log(NNrAN[a]);
                    }

                    //Update entity numbers
                    for (int b = 0; b < NNrAN.Length; b++)
                    {
                        _entityLookupTable[column, row, b] = Mathf.FloorToInt(NNrAN[b]);
                    }
                }
            }
        }

        public void CalculateMovement()
        {
            //We want to calculate the entity count change in each cell, THEN apply that difference to the entire grid

            //Perform this per-entity, following the maths provided
            for (int i = 0; i < EntityTypeCount; i++)
            {
                Entity entity = _entityTypeList[i];

                if (entity.MovementRate > 0f)
                {
                    int[,,] movementTable = new int[_entityLookupTable.GetLongLength(0), _entityLookupTable.GetLongLength(1), _entityLookupTable.GetLongLength(2)];

                    //Move our requested entity in each cell
                    for (int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
                    {
                        for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                        {
                            int currentPopulation = _entityLookupTable[column, row, i];
                            int neighbouringCellCount = GetValidNeighbourCount(column, row, i);

                            if (currentPopulation < 0)
                            {
                                //This cell/entity is empty, do not perform movement calculations
                                continue;
                            }

                            //TODO(caspar): Binomial?
                            //get number of entities to move
                            int entitiesToMove = 0;
                            for (int j = 0; j < currentPopulation; j++)
                            {
                                entitiesToMove += UnityEngine.Random.Range(0f, 1f) <= entity.MovementRate ? 1 : 0;
                            }

                            //divide moving entities by the number of valid neighbours
                            int entitiesMovingPerCell = Mathf.FloorToInt((float)entitiesToMove / (float)neighbouringCellCount);

                            //Add to neighbouring cells and remove from current cell respectively
                            for (int x = -1; x < 2; x++)
                            {
                                for (int y = -1; y < 2; y++)
                                {
                                    int xPos = column + x;
                                    int yPos = row + y;

                                    if (x == 0 && y == 0)
                                    {
                                        movementTable[xPos, yPos, i] -= (entitiesMovingPerCell * neighbouringCellCount);
                                    }
                                    else if (xPos >= 0 &&
                                            xPos < _entityLookupTable.GetLongLength(0) &&
                                            yPos >= 0 &&
                                            yPos < _entityLookupTable.GetLongLength(1))
                                    {
                                        //Check for valid cell
                                        if (_entityLookupTable[xPos, yPos, i] >= 0)
                                        {
                                            movementTable[xPos, yPos, i] += entitiesMovingPerCell;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Apply the movementTable numbers to our actual cells
                    for (int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
                    {
                        for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                        {
                            //Check for valid cell
                            if (_entityLookupTable[column, row, i] >= 0)
                            {
                                _entityLookupTable[column, row, i] = Mathf.Max(_entityLookupTable[column, row, i] + movementTable[column, row, i], 0);
                            }
                        }
                    }
                }
            }
        }

        private int GetValidNeighbourCount(int column, int row, int entity)
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
        #endregion
    }
}
