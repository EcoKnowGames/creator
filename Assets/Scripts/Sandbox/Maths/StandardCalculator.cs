using System.Linq;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public class StandardCalculator : IEntityCalculator
    {
        public string Name() => "Standard";
        public string Version() => "1.0";

        private readonly IMovementModel _movement = new NeighbourDispersal(zoneAware: true);

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

                    int zone = entityManager.GetZoneType(column, row);
                    float[,] A = entityManager.GetAlphaMatrixForZone(zone);
                    if ((A.GetLongLength(0) != entityManager.EntityTypeCount) || (A.GetLongLength(1) != entityManager.EntityTypeCount))
                    {
                        Debug.LogError($"[{Name()} Calculator] Entity count [{entityList.Length}] does not match the entity count of the Alpha Matrix. Aborting calculations...");
                        return;
                    }

                    float[] r = entityManager.GetEntityTypeList().Select(x => x.ZoneInformation != null && x.ZoneInformation.FirstOrDefault(z => z.ZoneID == zone) != null ? x.ZoneInformation.FirstOrDefault(z => z.ZoneID == zone).GrowthRate : x.GrowthRate).ToArray();
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
                        NNrAN[a] = N[a] + (N[a] * (r[a] + AN[a]));
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

            _movement.Move(entityManager, entityLookupTable);
        }
    }
}
