using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    public class NeighbourDispersal : IMovementModel
    {
        private readonly bool _zoneAware;

        public NeighbourDispersal(bool zoneAware = true)
        {
            _zoneAware = zoneAware;
        }

        public string Name() => _zoneAware ? "Neighbour Dispersal" : "Neighbour Dispersal (zone-unaware)";

        public void Move(EntityManager entityManager, int[,,] entityLookupTable)
        {
            if (entityManager == null) return;   // silent guard; the calculators keep the logging guard
            if (_zoneAware) MoveZoneAware(entityManager, entityLookupTable);
            else MoveZoneUnaware(entityManager, entityLookupTable);
        }

        private void MoveZoneAware(EntityManager entityManager, int[,,] entityLookupTable)
        {
            //We want to calculate the entity count change in each cell, THEN apply that difference to the entire grid
            //Perform this per-entity, following the maths provided
            for (int i = 0; i < entityManager.EntityTypeCount; i++)
            {
                Entity entity = entityManager.GetEntityTypeList()[i];

                if (entity.MovementRate > 0f || (entity.ZoneInformation != null && entity.ZoneInformation.Any(x => x.MovementRate > 0f)))
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

                            int currentZone = entityManager.GetZoneType(column, row);
                            float movementRate = entity.ZoneInformation != null && entity.ZoneInformation.Any(x => x.ZoneID == currentZone) ? entity.ZoneInformation.FirstOrDefault(x => x.ZoneID == currentZone).MovementRate : entity.MovementRate;

                            if (movementRate <= 0f)
                            {
                                continue;
                            }

                            // Get valid neighbors for this cell, filtering by zone transitions
                            List<Vector2Int> validNeighbors = DispersalSampling.GetValidNeighbors(entityManager, entityLookupTable, column, row, i, entity, currentZone);
                            int neighbouringCellCount = validNeighbors.Count;

                            if (neighbouringCellCount == 0)
                            {
                                continue;
                            }

                            // 1. Sample number of movers from Binomial distribution
                            int entitiesToMove = DispersalSampling.Binomial(currentPopulation, movementRate);

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

        private void MoveZoneUnaware(EntityManager entityManager, int[,,] entityLookupTable)
        {
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
