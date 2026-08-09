using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Shared stochastic dispersal sampling helpers extracted from the individual
    /// entity calculators (StandardCalculator, StructuredCalculator,
    /// StandardWraparoundCalculator) for Issue #56.
    ///
    /// This is a pure relocation: the method bodies were copied verbatim from the
    /// calculators. Behaviour is unchanged, including UnityEngine.Random call order,
    /// so simulation results are identical to before the extraction.
    /// </summary>
    public static class DispersalSampling
    {
        // Binomial sampling with n=1000 threshold compromise
        public static int Binomial(int n, float p)
        {
            if (n <= 0 || p <= 0) return 0;
            if (p >= 1) return n;

            // Use normal approximation for large populations (n >= 1000)
            if (n >= 1000)
            {
                float mean = n * p;
                float stdDev = Mathf.Sqrt(n * p * (1 - p));
                float sample = Normal(mean, stdDev);
                return Mathf.Clamp(Mathf.RoundToInt(sample), 0, n);
            }
            // Use exact binomial sampling for small populations (n < 1000)
            else
            {
                int successes = 0;
                for (int i = 0; i < n; i++)
                {
                    if (Random.Range(0f, 1f) <= p)
                    {
                        successes++;
                    }
                }
                return successes;
            }
        }

        // Normal distribution sampling using Box-Muller transform
        public static float Normal(float mean, float stdDev)
        {
            float u1 = 1.0f - Random.Range(0f, 1f);
            float u2 = 1.0f - Random.Range(0f, 1f);
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        // Multinomial sampling for distributing movers among neighbors
        public static int[] Multinomial(int totalMovers, int numberOfCategories)
        {
            int[] results = new int[numberOfCategories];

            if (numberOfCategories == 1)
            {
                results[0] = totalMovers;
                return results;
            }

            // Distribute movers one by one to random neighbors
            for (int i = 0; i < totalMovers; i++)
            {
                int chosenNeighbor = Random.Range(0, numberOfCategories);
                results[chosenNeighbor]++;
            }

            return results;
        }

        // Helper method to get valid neighbor coordinates, with zone transition filtering
        public static List<Vector2Int> GetValidNeighbors(EntityManager entityManager, int[,,] entityLookupTable, int column, int row, int entityIndex, Entity entity = null, int currentZone = 0)
        {
            List<Vector2Int> validNeighbors = new List<Vector2Int>();

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    int xPos = column + x;
                    int yPos = row + y;

                    // Skip the center cell and check boundaries
                    if ((x == 0 && y == 0) ||
                        xPos < 0 || xPos >= entityLookupTable.GetLongLength(0) ||
                        yPos < 0 || yPos >= entityLookupTable.GetLongLength(1))
                    {
                        continue;
                    }

                    // Check if the target cell is valid for this entity
                    if (entityLookupTable[xPos, yPos, entityIndex] >= 0)
                    {
                        // Check zone transition rules
                        if (entity != null && entity.ZoneInformation != null && entity.ZoneInformation.Length > 0)
                        {
                            int neighbourZone = entityManager.GetZoneType(xPos, yPos);

                            bool canTransition = entity.ZoneInformation.Any(x => x.ZoneID == currentZone && x.Transitions.Contains(neighbourZone));
                            if (!canTransition)
                            {
                                continue;
                            }
                        }

                        validNeighbors.Add(new Vector2Int(xPos, yPos));
                    }
                }
            }

            return validNeighbors;
        }
    }
}
