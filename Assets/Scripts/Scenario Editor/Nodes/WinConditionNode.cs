using System;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

public record WinConditionRecord
(
    string Title,
    string Description,
    int TypeIndex,
    int TargetIndex,
    float LowerLimit,
    float UpperLimit,
    int RequiredRounds,

    // Authoritative target (Entity/Item ID, or PlayerInventory.CurrencyID). Older JSONs omit it;
    // ScenarioLoader.NormaliseWinConditionTargets fills it from the indices on load.
    string TargetId = null,

    // Legacy, read-only: the pre-11-Mar-2026 key (commit 4054a39 renamed it to TypeIndex/TargetIndex
    // without a migration). Converted to TargetId on load by ScenarioLoader.NormaliseWinConditionTargets,
    // then cleared so it is never re-serialised. Repo scenarios still using it as of this change:
    // Autoplace Example, Emissions, Hares Foxes and Houses, Rock Paper Scissors, example_scenario.
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] int? EntityIndex = null
);

public class WinConditionNode : Node
{
    [Output(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private string _id;


    [SerializeField] protected string title;
    [SerializeField, Multiline]
    protected string description;

    [SerializeField, HideInInspector] protected int typeIndex;
    public int TypeIndex { get { return typeIndex; } set { typeIndex = value; } }

    //Legacy: kept only to migrate graphs saved before #76 (FormerlySerializedAs recovers the older entityIndex field too).
    //targetId is authoritative; this is never read once targetId is set.
    [SerializeField, HideInInspector, FormerlySerializedAs("entityIndex")] protected int targetIndex;
    public int TargetIndex { get { return targetIndex; } set { targetIndex = value; } }

    //Authoritative target: Entity/Item ID, or PlayerInventory.CurrencyID
    [SerializeField, HideInInspector] protected string targetId;
    public string TargetId
    {
        get
        {
            if (!string.IsNullOrEmpty(targetId))
            {
                return targetId;
            }

            //Lazy migration of graphs saved before #76: derive (without writing) from the legacy index
            return GetLegacyTargetId();
        }
        set { targetId = value; }
    }

    [SerializeField] protected float lowerLimit;
    [SerializeField] protected float upperLimit;

    [SerializeField] protected int requiredRounds = 1; //consistency across rounds

    public List<Entity> AvailableEntities
    {
        get
        {
            //Are we connected to the main scenario node?
            NodePort outputPort = GetOutputPort("_id");
            if ((outputPort != null) &&
                (outputPort.IsConnected) &&
                (outputPort.Connection.node.GetType() == typeof(ScenarioNode)))
            {
                //Return valid entity list from graph
                if (graph is ScenarioNodeGraph scenarioGraph)
                {
                    return scenarioGraph.GetEntityList();
                }
            }

            return null;
        }
    }

    public List<Item> AvailableItems
    {
        get
        {
            //Return valid item list from graph
            if (graph is ScenarioNodeGraph scenarioGraph)
            {
                return scenarioGraph.GetScenarioNode()?.ItemDefs;
            }

            return null;
        }
    }

    protected override void Init()
    {
        base.Init();
    }

    private void OnValidate()
    {
        NodePort inputPort = GetInputPort("_id");
        if (inputPort != null)
        {
            if (inputPort.IsConnected)
            {
                Debug.Log($"input port type = {inputPort.Connection.node.GetType()}");
            }
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.IsInput)
        {
            Debug.Log($"input port type = {port.GetType()}");
        }

        return null;
    }

    //Uses the orders the pre-#76 popups used: Entities in graph node order, Items ID-sorted (ScenarioNode.ItemDefs)
    private string GetLegacyTargetId()
    {
        if (targetIndex < 0)
        {
            return null;
        }

        switch ((WinCondition.TargetType)typeIndex)
        {
            case (WinCondition.TargetType.Entity):
                {
                    List<Entity> entities = AvailableEntities;
                    return ((entities != null) && (targetIndex < entities.Count)) ? entities[targetIndex]?.ID : null;
                }
            case (WinCondition.TargetType.Item):
                {
                    List<Item> items = AvailableItems;
                    return ((items != null) && (targetIndex < items.Count)) ? items[targetIndex]?.ID : null;
                }
            case (WinCondition.TargetType.Currency):
                return PlayerInventory.CurrencyID;
            default:
                return null;
        }
    }

    //Index of TargetId in the current list for its type (-1 if not found). Derived, never stored as the reference.
    public int GetTargetIndex()
    {
        string id = TargetId;
        if (string.IsNullOrEmpty(id))
        {
            return -1;
        }

        switch ((WinCondition.TargetType)typeIndex)
        {
            case (WinCondition.TargetType.Entity):
                {
                    List<Entity> entities = AvailableEntities;
                    return entities == null ? -1 : entities.FindIndex(x => (x != null) && string.Equals(x.ID, id, StringComparison.OrdinalIgnoreCase));
                }
            case (WinCondition.TargetType.Item):
                {
                    List<Item> items = AvailableItems;
                    return items == null ? -1 : items.FindIndex(x => (x != null) && string.Equals(x.ID, id, StringComparison.OrdinalIgnoreCase));
                }
            case (WinCondition.TargetType.Currency):
                return 0;
            default:
                return -1;
        }
    }

    public WinConditionRecord GetWinCondition()
    {
        //TargetIndex is exported only for older readers; ScenarioLoader prefers TargetId
        return new WinConditionRecord(title, description, typeIndex, GetTargetIndex(), lowerLimit, upperLimit, requiredRounds, TargetId);
    }

    public bool IsConnected()
    {
        NodePort port = GetOutputPort("_id");
        if ((port != null) && (port.IsConnected))
        {
            if (port.Connection.node is ScenarioNode scenario)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsRangeValid()
    {
        if ((upperLimit > 0) && (upperLimit < lowerLimit))
        {
            return false;
        }

        return true;
    }
}
