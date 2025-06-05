using System;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public record Entity(
        string ID,
        float GrowthRate,
        float MovementRate
        );

    /*public record Matrix(
        string[] EntityIDs,
        float[,] EntityAlphas 
        );*/

    public delegate void EntityEvent(int column, int row, int id);

    public class EntityEvents
    {
        public EntityEvent OnEntityHarvested;
        public EntityEvent OnEntityIntroduced;
    }


    //Note(caspar) -> This class stores and handles manipulation of the Entity data
    //Data can be requested or modified here
    public class EntityManager : MonoBehaviour
    {
        //private float[,] _alphaMatrix;
        private Matrix _entityMatrix;
        public float[,] AlphaMatrix => _entityMatrix.entityMatrix;
        private Entity[] _entityTypeList;
        public int EntityTypeCount => _entityTypeList.Length;


        //Cell lookup table?
        //X, Y, entityIndex
        private int[,,] _entityLookupTable;

        public EntityEvents entityEvents = new EntityEvents();

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

        public void AddEntitiesToGrid(GridManager gridManager)
        {
            if (gridManager == null)
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
                        _entityLookupTable[column, row, i] = gridManager.FindCellAtPosition(column, row) != null ? 100 : -1;
                    }
                }
            }

            gridManager.UpdateAllCells();
        }
        #endregion

        public CellEntity[] GetEntitiesForCell(int column, int row)
        {
            CellEntity[] entityCounts = new CellEntity[EntityTypeCount];

            for (int i = 0; i < EntityTypeCount; i++)
            {
                string id = _entityTypeList[i].ID;
                int population = _entityLookupTable[column, row, i];

                entityCounts[i] = new CellEntity(id, population);
            }

            return entityCounts;
        }

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
                int currentPopulation = _entityLookupTable[column, row, index];
                int newPopulation = Mathf.Max(currentPopulation - amount, 0);

                //TODO(caspar): We cannot harvest more than we have in the cell, so what sort of user feedback should we get if we try to harvest too much?

                int difference = newPopulation - currentPopulation;

                Debug.Log($"{LogChannel} [HARVEST Entity {index}] Current: {currentPopulation} / New: {newPopulation} / Difference: {difference}");

                _entityLookupTable[column, row, index] = newPopulation;
                entityEvents?.OnEntityHarvested?.Invoke(column, row, index);

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
                int currentPopulation = _entityLookupTable[column, row, index];
                int newPopulation = currentPopulation + amount;

                int difference = newPopulation - currentPopulation;

                Debug.Log($"{LogChannel} [INTRODUCE Entity {index}] Current: {currentPopulation} / New: {newPopulation} / Difference: {difference}");

                _entityLookupTable[column, row, index] = newPopulation;
                entityEvents?.OnEntityIntroduced?.Invoke(column, row, index);

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
                                        movementTable[xPos, yPos, i] += entitiesMovingPerCell;
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
                            _entityLookupTable[column, row, i] = Mathf.Max(_entityLookupTable[column, row, i] + movementTable[column, row, i], 0);
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
