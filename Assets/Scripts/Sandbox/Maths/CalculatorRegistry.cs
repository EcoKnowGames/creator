using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glitchers.EcoKnow.Sandbox
{
    /// <summary>
    /// Single source of truth for the population calculators a scenario can use.
    ///
    /// To add a new model: implement IEntityCalculator, then add one entry to the
    /// list below. The stable Id is written into scenario data; DisplayName is shown
    /// in the node-editor dropdown. LegacyIds lets an Id be renamed later without
    /// breaking scenarios that were saved with the old id. (Issue #58)
    /// </summary>
    public static class CalculatorRegistry
    {
        /// <summary>Calculator used when a scenario specifies none, or an unknown id.</summary>
        public const string DefaultId = "lotka-volterra";

        private static readonly List<CalculatorInfo> Calculators = new List<CalculatorInfo>
        {
            new CalculatorInfo(
                id: "lotka-volterra",
                displayName: "Lotka-Volterra",
                factory: () => new StandardCalculator()),
            new CalculatorInfo(
                id: "lotka-volterra-wraparound",
                displayName: "Lotka-Volterra (Wraparound)",
                factory: () => new StandardWraparoundCalculator()),
            new CalculatorInfo(
                id: "structured",
                displayName: "Structured",
                factory: () => new StructuredPopCalculator()),
            new CalculatorInfo(
                id: "logistic",
                displayName: "Logistic Growth",
                factory: () => new LogisticCalculator()),
        };

        /// <summary>All registered calculators, in registration order (for dropdowns).</summary>
        public static IReadOnlyList<CalculatorInfo> All => Calculators;

        /// <summary>True if the id matches a registered calculator's id or one of its legacy ids.</summary>
        public static bool Contains(string id) => Find(id) != null;

        /// <summary>
        /// Create a fresh calculator instance for the given id. A null or empty id means
        /// "no calculator specified" (for example a scenario saved before the field
        /// existed) and resolves to the default silently. A non-empty but unknown id
        /// falls back to the default and logs a warning.
        /// </summary>
        public static IEntityCalculator Create(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Find(DefaultId).Factory();
            }

            CalculatorInfo info = Find(id);
            if (info == null)
            {
                Debug.LogWarning($"[CalculatorRegistry] Unknown calculator id '{id}'. Falling back to default '{DefaultId}'.");
                info = Find(DefaultId);
            }
            return info.Factory();
        }

        private static CalculatorInfo Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            return Calculators.FirstOrDefault(c => c.Id == id || c.LegacyIds.Contains(id));
        }
    }

    /// <summary>Registry entry describing one selectable calculator.</summary>
    public class CalculatorInfo
    {
        /// <summary>Stable identifier written into scenario data. Do not change without a legacy id.</summary>
        public string Id { get; }

        /// <summary>Human-readable label shown in the node-editor dropdown.</summary>
        public string DisplayName { get; }

        /// <summary>Creates a fresh instance of this calculator.</summary>
        public Func<IEntityCalculator> Factory { get; }

        /// <summary>Former ids that should still resolve to this calculator (for renames).</summary>
        public IReadOnlyList<string> LegacyIds { get; }

        public CalculatorInfo(string id, string displayName, Func<IEntityCalculator> factory, IReadOnlyList<string> legacyIds = null)
        {
            Id = id;
            DisplayName = displayName;
            Factory = factory;
            LegacyIds = legacyIds ?? Array.Empty<string>();
        }
    }
}
