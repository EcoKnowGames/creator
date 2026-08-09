using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox.Grid;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Example calculator: single-species logistic (density-dependent) growth.
    ///
    /// Each entity grows toward a carrying capacity K on its own, with no interaction
    /// (alpha) matrix:
    ///     N = N + r * N * (1 - N / K)
    /// This makes it a minimal template for adding a new population model: implement
    /// IEntityCalculator, do the per-cell maths in CalculatePopulations, and reuse an
    /// existing IMovementModel for dispersal. Registered in CalculatorRegistry. See #61.
    ///
    /// The scenario schema has no carrying-capacity field yet, so K is a documented
    /// placeholder derived from each entity's StartPopulation. A production model would add
    /// a real per-entity K.
    /// </summary>
    public class LogisticCalculator : IEntityCalculator
    {
        public string Name() => "Logistic Growth";
        public string Version() => "1.0";

        // Movement is composed, not reimplemented: reuse the shared 8-neighbour dispersal.
        private readonly IMovementModel _movement = new NeighbourDispersal(zoneAware: true);

        // Placeholder carrying capacity. K = StartPopulation * this multiplier, with a
        // fallback for entities whose StartPopulation is 0. Replace with a real per-entity
        // parameter for production use.
        private const float CarryingCapacityMultiplier = 4f;
        private const float DefaultCarryingCapacity = 100f;

        public void CalculatePopulations(EntityManager entityManager, int[,,] entityLookupTable)
        {
            if (entityManager == null)
            {
                Debug.LogError($"[{Name()} Calculator] EntityManager is null! Aborting calculations...");
                return;
            }

            Entity[] entityTypes = entityManager.GetEntityTypeList();

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
                        continue;
                    }

                    int zone = entityManager.GetZoneType(column, row);

                    // Per-entity growth rate r, zone-aware (same selection the other calculators use).
                    float[] r = entityTypes.Select(x => x.ZoneInformation != null && x.ZoneInformation.FirstOrDefault(z => z.ZoneID == zone) != null ? x.ZoneInformation.FirstOrDefault(z => z.ZoneID == zone).GrowthRate : x.GrowthRate).ToArray();

                    // Current population N per entity in this cell.
                    float[] N = entityList.Select(x => (float)x.Population).ToArray();

                    // Logistic update per entity, independent of the others (no interaction matrix):
                    //     N = N + r * N * (1 - N / K)
                    for (int a = 0; a < N.Length; a++)
                    {
                        float startPopulation = entityTypes[a].StartPopulation;
                        float k = startPopulation > 0f ? startPopulation * CarryingCapacityMultiplier : DefaultCarryingCapacity;

                        float next = N[a] + (r[a] * N[a] * (1f - (N[a] / k)));
                        entityLookupTable[column, row, a] = Mathf.Max(0, Mathf.FloorToInt(next));
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
