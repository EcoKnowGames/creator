using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Glitchers.EcoKnow.Sandbox.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType { GAME_START, GAME_END, ROUND_END, INTRODUCE, HARVEST, SELL };

    #region Meta Data Objects
    public abstract class MetaDataObject
    {
        public abstract string Key { get; }
        public System.Object Object;

        public MetaDataObject(System.Object obj)
        {
            Object = obj;
        }
    };

    public class InventoryMetaDataObject : MetaDataObject
    {
        public override string Key => "InventoryChange";
        public InventoryMetaDataObject(System.Object obj) : base(obj) { }
    }

    public class CellMetaDataObject : MetaDataObject
    {
        public override string Key => "CellChange";
        public CellMetaDataObject(System.Object obj) : base(obj) { }
    }
    #endregion

    public record CalculatorDataObject
        (
            string Name,
            string Version
        );

    public record CellDataObject
        (
            int X,
            int Y,
            Dictionary<string, int> Populations
        );

    public record WinConditionDataObject
        (
            string Title,
            bool Completed,
            Dictionary<int, string> Results
        );

    public record EventDataObject
        (
            EventType Type,
            int Player,
            int Round,
            int Action,
            Dictionary<string, int> Populations,
            Dictionary<string, int> Inventory,
            List<WinConditionDataObject> WinConditions,
            List<CellDataObject> Map,
            Dictionary<string, System.Object> Meta
        );

    public record GameDataObject
        (
            ScenarioConfig Config,
            CalculatorDataObject Calculator,
            EventDataObject[] Events
        );
}
