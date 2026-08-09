using System.Collections.Generic;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Example calculator: the classic wolf-sheep-grass predation model, agent-based (#62).
    ///
    /// Unlike the count-based calculators (Lotka-Volterra, Logistic), this simulates
    /// INDIVIDUAL wolves and sheep. No agent knows anything about the global population; the
    /// familiar predator-prey boom-bust emerges purely from local rules:
    ///   - Sheep wander, lose energy each step, eat grass in their cell to gain energy,
    ///     reproduce when their energy is high, and die at zero energy.
    ///   - Wolves wander, lose energy each step, eat a sheep sharing their cell to gain energy,
    ///     reproduce when their energy is high, and die at zero energy.
    ///
    /// Bridging agents onto the engine: the engine stores per-cell population COUNTS in
    /// entityLookupTable[column, row, entityIndex] and reads them for the grid, graph and win
    /// conditions. This calculator keeps its wolves and sheep as instance state (the instance
    /// persists across rounds and is recreated per scenario), and each round it:
    ///   1. reconciles its agent lists to the table counts (the table is the round-start source
    ///      of truth, so this both seeds agents on the first round and absorbs any player
    ///      harvest/introduce edits),
    ///   2. runs the local agent rules, then
    ///   3. writes the resulting per-cell counts back into the table.
    /// Agents move themselves, so the movement phase uses NoMovement (a no-op).
    ///
    /// Grass is a per-cell food field: sheep consume it when they graze (its count drops), which
    /// is what lets them gain energy and reproduce. It does not regrow here - the grazing step is
    /// the hook where grass could be topped up over time (see StepAgents).
    ///
    /// Requires a scenario with three entities whose IDs are Grass, Sheep and Wolf (the
    /// interaction matrix values are ignored, but the matrix must list them so indices resolve).
    /// </summary>
    public class WolfSheepCalculator : IEntityCalculator
    {
        public string Name() => "Wolf-Sheep-Grass (Agent-Based)";
        public string Version() => "1.0";

        // Movement is per-agent (a random walk) done inside CalculatePopulations, so the
        // separate movement phase does nothing.
        private readonly IMovementModel _movement = new NoMovement();

        // Entity roles are resolved by ID each round (case-insensitive).
        private const string GrassId = "Grass";
        private const string SheepId = "Sheep";
        private const string WolfId = "Wolf";

        // Tunable rules (illustrative; tune per scenario/grid to get clean oscillations).
        private const float WolfInitialEnergy = 15f;
        private const float WolfStepCost = 2f;
        private const float WolfGainFromFood = 10f;
        private const float WolfReproduceThreshold = 30f;

        private const float SheepInitialEnergy = 12f;
        private const float SheepStepCost = 1f;
        private const float SheepGainFromGrass = 4f;
        private const float SheepReproduceThreshold = 20f;

        // Grass eaten per sheep per graze. Grass is a finite resource here and does not regrow.
        private const int GrassEatenPerGraze = 1;

        // Guards runaway growth (the grass-free-style sheep can explode); excess agents dropped.
        private const int MaxAgentsSafetyCap = 50000;

        private sealed class Agent
        {
            public int Column;
            public int Row;
            public float Energy;

            public Agent(int column, int row, float energy)
            {
                Column = column;
                Row = row;
                Energy = energy;
            }
        }

        private readonly List<Agent> _sheep = new List<Agent>();
        private readonly List<Agent> _wolves = new List<Agent>();

        private bool _warnedMissingEntities;
        private bool _warnedCap;

        public void CalculatePopulations(EntityManager entityManager, int[,,] entityLookupTable)
        {
            if (entityManager == null)
            {
                Debug.LogError($"[{Name()} Calculator] EntityManager is null! Aborting calculations...");
                return;
            }

            int grassIndex = entityManager.GetEntityIndex(GrassId);
            int sheepIndex = entityManager.GetEntityIndex(SheepId);
            int wolfIndex = entityManager.GetEntityIndex(WolfId);

            if (grassIndex < 0 || sheepIndex < 0 || wolfIndex < 0
                || grassIndex == sheepIndex || grassIndex == wolfIndex || sheepIndex == wolfIndex)
            {
                if (!_warnedMissingEntities)
                {
                    Debug.LogWarning($"[{Name()} Calculator] Expected three distinct entities with IDs '{GrassId}', '{SheepId}' and '{WolfId}'. Leaving populations unchanged.");
                    _warnedMissingEntities = true;
                }
                return;
            }

            int columns = (int)entityLookupTable.GetLongLength(0);
            int rows = (int)entityLookupTable.GetLongLength(1);

            // 1. Reconcile the agent lists to the table counts (source of truth). Seeds on the
            //    first round and absorbs any player harvest/introduce made between rounds.
            Reconcile(_sheep, entityLookupTable, sheepIndex, columns, rows, SheepInitialEnergy);
            Reconcile(_wolves, entityLookupTable, wolfIndex, columns, rows, WolfInitialEnergy);

            // 2. Run the agent-based rules (mutates the lists only).
            StepAgents(entityManager, entityLookupTable, grassIndex, sheepIndex, wolfIndex, columns, rows);

            // 3. Write per-cell counts back so the grid, graph and win conditions reflect it.
            WriteBack(entityLookupTable, sheepIndex, wolfIndex, columns, rows);
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

        private static int CellKey(int column, int row, int columns) => (row * columns) + column;

        // Rebuilds the agent list so its per-cell counts match the table, preserving the energy
        // of surviving agents. Off-map cells (value < 0) are skipped and never populated.
        private void Reconcile(List<Agent> agents, int[,,] table, int index, int columns, int rows, float initialEnergy)
        {
            Dictionary<int, List<Agent>> buckets = new Dictionary<int, List<Agent>>();
            foreach (Agent a in agents)
            {
                int key = CellKey(a.Column, a.Row, columns);
                if (!buckets.TryGetValue(key, out List<Agent> bucket))
                {
                    bucket = new List<Agent>();
                    buckets[key] = bucket;
                }
                bucket.Add(a);
            }

            List<Agent> reconciled = new List<Agent>();
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    int tableCount = table[column, row, index];
                    if (tableCount < 0)
                    {
                        continue; // off-map cell
                    }

                    buckets.TryGetValue(CellKey(column, row, columns), out List<Agent> bucket);
                    int have = bucket != null ? bucket.Count : 0;

                    int keep = Mathf.Min(have, tableCount);
                    for (int i = 0; i < keep; i++)
                    {
                        reconciled.Add(bucket[i]); // existing agent, keeps its energy
                    }
                    for (int i = keep; i < tableCount; i++)
                    {
                        reconciled.Add(new Agent(column, row, Random.Range(1f, 2f * initialEnergy)));
                    }
                    // Any surplus (player harvested this cell) is simply not carried over.
                }
            }

            agents.Clear();
            agents.AddRange(reconciled);
        }

        private void StepAgents(EntityManager em, int[,,] table, int grassIndex, int sheepIndex, int wolfIndex, int columns, int rows)
        {
            // a. Movement: each agent random-walks to a valid neighbouring cell (or stays put).
            MoveAgents(_sheep, em, table, sheepIndex);
            MoveAgents(_wolves, em, table, wolfIndex);

            // b. Energy upkeep.
            foreach (Agent s in _sheep) s.Energy -= SheepStepCost;
            foreach (Agent w in _wolves) w.Energy -= WolfStepCost;

            // c. Grazing: a sheep on a cell with grass eats some (consuming it) and gains energy.
            //    Grass is finite per cell and shared: sheep are processed in order, so once a
            //    cell's grass runs out the sheep still standing there gain nothing and must roam
            //    to find fresh grass.
            //    HOOK: grass is consumed but does NOT regrow here. To make it fully dynamic, add a
            //    regrowth pass that tops grass back up over time (for example logistic growth as in
            //    LogisticCalculator), combining the count-based (grass) and agent-based mechanics.
            foreach (Agent s in _sheep)
            {
                if (table[s.Column, s.Row, grassIndex] > 0)
                {
                    s.Energy += SheepGainFromGrass;
                    table[s.Column, s.Row, grassIndex] = Mathf.Max(0, table[s.Column, s.Row, grassIndex] - GrassEatenPerGraze);
                }
            }

            // d. Predation: each wolf eats at most one sheep sharing its cell.
            Dictionary<int, List<Agent>> sheepByCell = new Dictionary<int, List<Agent>>();
            foreach (Agent s in _sheep)
            {
                int key = CellKey(s.Column, s.Row, columns);
                if (!sheepByCell.TryGetValue(key, out List<Agent> bucket))
                {
                    bucket = new List<Agent>();
                    sheepByCell[key] = bucket;
                }
                bucket.Add(s);
            }

            HashSet<Agent> eaten = new HashSet<Agent>();
            foreach (Agent w in _wolves)
            {
                if (sheepByCell.TryGetValue(CellKey(w.Column, w.Row, columns), out List<Agent> bucket) && bucket.Count > 0)
                {
                    Agent prey = bucket[bucket.Count - 1];
                    bucket.RemoveAt(bucket.Count - 1);
                    eaten.Add(prey);
                    w.Energy += WolfGainFromFood;
                }
            }
            if (eaten.Count > 0)
            {
                _sheep.RemoveAll(s => eaten.Contains(s));
            }

            // e. Deaths from starvation.
            _sheep.RemoveAll(s => s.Energy <= 0f);
            _wolves.RemoveAll(w => w.Energy <= 0f);

            // f. Reproduction: an agent over its threshold splits (energy halved, child same cell).
            ReproduceOverThreshold(_sheep, SheepReproduceThreshold);
            ReproduceOverThreshold(_wolves, WolfReproduceThreshold);

            // g. Bound worst-case memory/time.
            EnforceCap(_sheep);
            EnforceCap(_wolves);
        }

        private void MoveAgents(List<Agent> agents, EntityManager em, int[,,] table, int index)
        {
            foreach (Agent a in agents)
            {
                List<Vector2Int> neighbours = DispersalSampling.GetValidNeighbors(em, table, a.Column, a.Row, index);
                if (neighbours.Count > 0)
                {
                    Vector2Int next = neighbours[Random.Range(0, neighbours.Count)];
                    a.Column = next.x;
                    a.Row = next.y;
                }
            }
        }

        private static void ReproduceOverThreshold(List<Agent> agents, float threshold)
        {
            int originalCount = agents.Count; // children added this round do not reproduce again
            for (int i = 0; i < originalCount; i++)
            {
                Agent a = agents[i];
                if (a.Energy >= threshold)
                {
                    a.Energy *= 0.5f;
                    agents.Add(new Agent(a.Column, a.Row, a.Energy));
                }
            }
        }

        private void EnforceCap(List<Agent> agents)
        {
            if (agents.Count > MaxAgentsSafetyCap)
            {
                agents.RemoveRange(MaxAgentsSafetyCap, agents.Count - MaxAgentsSafetyCap);
                if (!_warnedCap)
                {
                    Debug.LogWarning($"[{Name()} Calculator] Agent safety cap ({MaxAgentsSafetyCap}) reached; excess agents dropped. Use a smaller grid or fewer starting animals.");
                    _warnedCap = true;
                }
            }
        }

        // Rebuilds the sheep and wolf slices from the agent positions. Only those two slices are
        // reset (and only on on-map cells), so off-map cells (-1), the grass slice (managed by the
        // grazing step), and any other species are left exactly as they were.
        private void WriteBack(int[,,] table, int sheepIndex, int wolfIndex, int columns, int rows)
        {
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (table[column, row, sheepIndex] >= 0) table[column, row, sheepIndex] = 0;
                    if (table[column, row, wolfIndex] >= 0) table[column, row, wolfIndex] = 0;
                }
            }

            foreach (Agent s in _sheep) table[s.Column, s.Row, sheepIndex]++;
            foreach (Agent w in _wolves) table[w.Column, w.Row, wolfIndex]++;
        }
    }
}
