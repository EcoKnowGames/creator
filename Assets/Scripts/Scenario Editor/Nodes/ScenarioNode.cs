using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

[NodeWidth(300)]
public class ScenarioNode : Node
{
    [SerializeField] protected string scenarioName;
    [SerializeField] protected string authorName;
    [SerializeField] protected int rounds;
    [SerializeField] protected int actionsPerRound;
    [SerializeField] protected int startCurrency;

    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private Matrix _matrix;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private MapLayout _map;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _winConditions;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _items;

    public Matrix Matrix
    {
        get
        {
            //Return valid matrix
            if ((GetInputPort("_matrix") != null) && (GetInputPort("_matrix").ConnectionCount > 0))
            {
                return GetInputPort("_matrix").GetConnections().Select(x => x.node).OfType<MatrixNode>().First().matrix;
            }

            return null;
        }
    }

    public MapLayout MapLayout => GetInputPort("_map") != null && GetInputPort("_map").GetConnections().Count > 0 ? (MapLayout)GetInputPort("_map").GetInputValue() : null;

    public List<WinConditionRecord> WinConditions
    {
        get
        {
            //Return valid win condition list
            if ((GetInputPort("_winConditions") != null) && (GetInputPort("_winConditions").ConnectionCount > 0))
            {
                return GetInputPort("_winConditions").GetConnections().Select(x => x.node).OfType<WinConditionNode>().Select(x => x.GetWinCondition()).ToList();
            }

            return null;
        }
    }

    public bool HasWinConditions => WinConditions != null && WinConditions.Count > 0;


    public List<Item> ItemDefs
    {
        get
        {
            //Return valid item list
            //This ensures that the item list is always in the "correct" order
            if ((GetInputPort("_items") != null) && (GetInputPort("_items").ConnectionCount > 0))
            {
                return GetInputPort("_items").GetConnections().Select(x => x.node).OfType<ItemNode>().Select(x => x.GetItem()).OrderBy(x => x.ID).ToList();
            }

            return null;
        }
    }

    public bool HasItemDefs => ItemDefs != null && ItemDefs.Count > 0;

    [SerializeField] private int _seed;
    public int Seed => _seed;

    private List<NodePort> _entityPorts = new List<NodePort>();
    public List<NodePort> EntityPorts => _entityPorts;

    public string Name => scenarioName;
    public string Author => string.IsNullOrEmpty(authorName) ? "Unknown Author" : authorName;
    public int TotalRounds => rounds;
    public int ActionsPerRound => actionsPerRound;

    public int StartCurrency => startCurrency;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();
    }

    private void OnValidate()
    {

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        foreach (NodePort entity in _entityPorts)
        {
            if (entity.fieldName == port.fieldName)
            {
                return entity.fieldName;
            }
        }

        return null;
    }

    public void ProcessMatrix()
    {
        if (GetInputPort("_matrix").IsConnected)
        {
            Matrix matrix = (Matrix)GetInputPort("_matrix").GetInputValue();
            ClearEntityPorts();

            if ((matrix != null) && (_entityPorts != null))
            {
                foreach (string id in matrix.entityIDs)
                {
                    if (_entityPorts.FirstOrDefault(x => x.fieldName.Equals(id)) == null)
                    {
                        _entityPorts.Add(AddDynamicOutput(typeof(string), fieldName: id, connectionType: ConnectionType.Override));
                    }
                }
            }
        }
        else
        {
            ClearEntityPorts();
        }

        Debug.Log("DYNAMIC PORTS: " + DynamicPorts.Count());
    }

    public bool DoesMapPopulationCountMatchMatrix()
    {
        if ((MapLayout != null) && (MapLayout.gridDef != null))
        {
            if (!MapLayout.gridDef.HasPopulations())
            {
                return true;
            }
            else
            {
                if (MapLayout.gridDef.GetValidEntityCount() != GetConnectedEntityCount())
                {
                    return false;
                }
            }
        }
        return true;
    }

    #region Entities
    public int GetConnectedEntityCount()
    {
        if (graph is ScenarioNodeGraph scenarioGraph)
        {
            return scenarioGraph.GetEntityList().Count;
        }

        return 0;
    }
    private void ClearEntityPorts()
    {
        if ((_entityPorts != null) && (_entityPorts.Count > 0))
        {
            foreach (NodePort port in _entityPorts)
            {
                RemoveDynamicPort(port);
            }
        }

        _entityPorts.Clear();
    }
    #endregion

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        if (port == GetInputPort("_matrix"))
        {
            ClearEntityPorts();
        }
    }
}
