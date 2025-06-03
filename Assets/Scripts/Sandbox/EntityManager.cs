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

    public class EntityManager : MonoBehaviour
    {
        //private float[,] _alphaMatrix;
        private Matrix _entityMatrix;
        public float[,] AlphaMatrix => _entityMatrix.entityMatrix;
        private List<Entity> _entityTypeList;
        //private Dictionary<string, Entity> _entityDictionary;
        public List<Entity> AllEntityTypes => _entityTypeList; //_entityDictionary == null ? null : _entityDictionary.Select(x => x.Value).ToList();


        //Cell lookup table?
        //X, Y, entityIndex
        private int[,,] _entityLookupTable;

        private const string LogChannel = "[EntityManager]";

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
            entityRecords = entityRecords.OrderBy(x => Array.IndexOf(_entityMatrix.entityIDs, x.ID)).ToList();

            _entityTypeList = entityRecords;

            //_entityDictionary = new Dictionary<string, Entity>();

            /*foreach (Entity record in entityRecords)
            {
                _entityDictionary.Add(record.ID, record);
            }*/
        }

        public void AddEntitiesToGrid(GridManager gridManager)
        {
            if (gridManager == null)
            {
                return;
            }

            Vector2 gridSize = gridManager.GridSize;
            _entityLookupTable = new int[(int)gridSize.x, (int)gridSize.y, _entityTypeList.Count];

            //Add arbritrary amount of entities to each cell for now
            for (int row = 0; row < gridSize.y; row++)
            {
                for (int column = 0; column < gridSize.x; column++)
                {
                    if (gridManager.FindCellAtPosition(column, row) != null)
                    {
                        for (int i = 0; i < _entityTypeList.Count; i++)
                        {
                            _entityLookupTable[column, row, i] = 100;
                        }
                    }
                }
            }

            gridManager.UpdateAllCells();
        }

        public List<CellEntity> GetEntitiesForCell(int column, int row)
        {
            List<CellEntity> entityCounts = new List<CellEntity>();

            for (int i = 0; i < _entityTypeList.Count; i++)
            {
                string id = _entityTypeList[i].ID;
                int population = _entityLookupTable[column, row, i];

                entityCounts.Add(new CellEntity(id, population));
            }

            return entityCounts;
        }


        //TODO(caspar): Cleanup to ensure IDs match and are in the right order when retrieving/setting the population at the end
        public void CalculateNewEntityCount()
        {
            for(int column = 0; column < _entityLookupTable.GetLongLength(0); column++)
            {
                for (int row = 0; row < _entityLookupTable.GetLongLength(1); row++)
                {
                    List<CellEntity> entityList = GetEntitiesForCell(column, row);

                    if ((entityList == null) || (entityList.Count <= 0))
                    {
                        Debug.LogError($"{LogChannel} No entities found for Cell [{row} , {column}]. Aborting calculations...");
                        return;
                    }

                    int entityCount = AllEntityTypes.Count;
                    if (entityList.Count != entityCount)
                    {
                        Debug.LogError($"{LogChannel} Entity count [{entityList.Count}] for Cell [{row} , {column}] does not match the Simulation Entity count [{entityCount}]! Aborting calculations...");
                        return;
                    }

                    float[,] A = AlphaMatrix;
                    if ((A.GetLongLength(0) != entityCount) || (A.GetLongLength(1) != entityCount))
                    {
                        Debug.LogError($"{LogChannel} Entity count [{entityList.Count}] does not match the entity count of the Alpha Matrix. Aborting calculations...");
                        return;
                    }

                    float[] r = AllEntityTypes.Select(x => x.GrowthRate).ToArray();
                    float[] N = entityList.Select(x => (float)x.Population).ToArray();

                    //AN
                    float[] AN = new float[entityCount];
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
                    float[] NNrAN = new float[entityCount];
                    for (int a = 0; a < N.Length; a++)
                    {
                        NNrAN[a] = N[a] + (N[a] * (r[a] + AN[a]));
                        //Debug.Log(NNrAN[a]);
                    }

                    //Update entity numbers
                    for (int b = 0; b < NNrAN.Length; b++)
                    {
                        //entityList[b].Population =
                        _entityLookupTable[column, row, b] = Mathf.FloorToInt(NNrAN[b]);
                    }
                }
            }
        }

        public void CalculateMovement()
        {

            //Calculate movementdata for all entities/cells

            //entityMovementData.Add(cell.CalculateMovementData());

            //NOW - Apply the entityMovementData numbers to our cells


            //We want to calculate the entity count change in each cell, THEN apply that difference to the entire grid
            //We will need to store:
            // X pos
            // Y pos
            // List of CellEntity (contains id and population change)




            //Cell[,] movementMatrix

            //And now we do our movement logic
            //We loop through entities rather than cells because if an entity moves to a new cell, it would be re-processed when that cell calculates movement
            /*List<Cell> flattenedCellList = cellList.Cast<Cell>().Where(x => x != null).ToList();
            List<CellEntity> allEntities = new List<CellEntity>();
            flattenedCellList.ForEach(x => allEntities.AddRange(x.ActiveEntities));

            foreach(CellEntity cellEntity in allEntities)
            {
                //Logic
            }*/
        }

    }
}
