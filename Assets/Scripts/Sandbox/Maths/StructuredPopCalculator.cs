using System.Linq;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public class StructuredPopCalculator : IEntityCalculator
    {
        public string Name() => "Structured";
        public string Version() => "1.0";

        public void CalculatePopulations(EntityManager entityManager, int[,,] entityLookupTable)
        {
            if (entityManager == null)
            {
                Debug.LogError($"[{Name()} Calculator] EntityManager is null! Aborting calculations...");
                return;
            }

            for (int column = 0; column < entityLookupTable.GetLongLength(0); column++)
            {
                for (int row = 0; row < entityLookupTable.GetLongLength(1); row++)
                {
                    CellEntity[] entityList = entityManager.GetEntitiesForCell(column, row);

                    if ((entityList == null) || (entityList.Length <= 0))
                    {
                        Debug.LogError($"[{Name()} Calculator] No entities found for Cell [{row} , {column}]. Aborting calculations...");
                        return;
                    }

                    if (entityList.Length != entityManager.EntityTypeCount)
                    {
                        Debug.LogError($"[{Name()} Calculator] Entity count [{entityList.Length}] for Cell [{row} , {column}] does not match the Simulation Entity count [{entityManager.EntityTypeCount}]! Aborting calculations...");
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

                    if (nullCount >= entityManager.EntityTypeCount)
                    {
                        //This cell is completely empty, don't bother calculating
                        //Debug.Log($"Row {row} / Column {column} is empty!");
                        continue;
                    }

                    float[,] A = entityManager.AlphaMatrix;
                    if ((A.GetLongLength(0) != entityManager.EntityTypeCount) || (A.GetLongLength(1) != entityManager.EntityTypeCount))
                    {
                        Debug.LogError($"[{Name()} Calculator] Entity count [{entityList.Length}] does not match the entity count of the Alpha Matrix. Aborting calculations...");
                        return;
                    }

                    float[] r = entityManager.GetEntityTypeList().Select(x => x.GrowthRate).ToArray();
                    float[] N = entityList.Select(x => (float)x.Population).ToArray();

                    //AN
                    float[] AN = new float[entityManager.EntityTypeCount];
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
                    float[] NNrAN = new float[entityManager.EntityTypeCount];
                    for (int a = 0; a < N.Length; a++)
                    {
                        NNrAN[a] = AN[a];
                        //Debug.Log(NNrAN[a]);
                    }

                    //Update entity numbers
                    for (int b = 0; b < NNrAN.Length; b++)
                    {
                        entityLookupTable[column, row, b] = Mathf.Max(0, Mathf.FloorToInt(NNrAN[b]));
                    }
                }
            }
        }

        public void CalculateMovement(EntityManager entityManager, int[,,] entityLookupTable)
        {
            if (entityManager == null)
            {
                Debug.LogError($"[{Name()} Calculator] EntityManager is null! Aborting calculations...");
                return;
            }

            //We want to calculate the entity count change in each cell, THEN apply that difference to the entire grid
            //Perform this per-entity, following the maths provided
            for (int i = 0; i < entityManager.EntityTypeCount; i++)
            {
                Entity entity = entityManager.GetEntityTypeList()[i];

                if (entity.MovementRate > 0f)
                {
                    int[,,] movementTable = new int[entityLookupTable.GetLongLength(0), entityLookupTable.GetLongLength(1), entityLookupTable.GetLongLength(2)];

                    //Move our requested entity in each cell
                    for (int column = 0; column < entityLookupTable.GetLongLength(0); column++)
                    {
                        for (int row = 0; row < entityLookupTable.GetLongLength(1); row++)
                        {
                            int currentPopulation = entityLookupTable[column, row, i];
                            if (currentPopulation < 0)
                            {
                                //This cell/entity is empty, do not perform movement calculations
                                continue;
                            }

                            // Get valid neighbors for this cell
                            List<Vector2Int> validNeighbors = DispersalSampling.GetValidNeighbors(entityManager, entityLookupTable, column, row, i);
                            int neighbouringCellCount = validNeighbors.Count;

                            if (neighbouringCellCount == 0) continue;

                            // 1. Sample number of movers from Binomial distribution
                            int entitiesToMove = DispersalSampling.Binomial(currentPopulation, entity.MovementRate);

                            if (entitiesToMove > 0)
                            {
                                // 2. Distribute movers among neighbors using equal probabilities
                                int[] moversPerNeighbor = DispersalSampling.Multinomial(entitiesToMove, neighbouringCellCount);

                                // 3. Apply movement to movementTable
                                movementTable[column, row, i] -= entitiesToMove;

                                for (int neighborIndex = 0; neighborIndex < neighbouringCellCount; neighborIndex++)
                                {
                                    Vector2Int neighbor = validNeighbors[neighborIndex];
                                    movementTable[neighbor.x, neighbor.y, i] += moversPerNeighbor[neighborIndex];
                                }
                            }
                        }
                    }

                    //Apply the movementTable numbers to our actual cells
                    for (int column = 0; column < entityLookupTable.GetLongLength(0); column++)
                    {
                        for (int row = 0; row < entityLookupTable.GetLongLength(1); row++)
                        {
                            //Check for valid cell
                            if (entityLookupTable[column, row, i] >= 0)
                            {
                                entityLookupTable[column, row, i] = Mathf.Max(0, entityLookupTable[column, row, i] + movementTable[column, row, i]);
                            }
                        }
                    }
                }
            }
        }
    }
}