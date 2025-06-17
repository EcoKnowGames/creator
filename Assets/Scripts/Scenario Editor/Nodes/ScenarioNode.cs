using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

/*public class Scenario
{
	public string name { get; }
	public int rounds { get; }
	public int actionsPerRound { get; }
	public int startCurrency { get; }

	public Scenario(ScenarioNode node)
	{
		name = node.Name;
		rounds = node.rounds;
	}
}*/

[NodeWidth(300)]
public class ScenarioNode : Node
{
    [SerializeField] protected string scenarioName;
    [SerializeField] protected int rounds;
    [SerializeField] protected int actionsPerRound;
    [SerializeField] protected int startCurrency;

    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private Matrix _matrix;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private MapLayout _map;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _winConditions;

    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private string _items;

    public List<Item> ItemDefs
    {
        get
        {
            //Return valid item list from graph
            //This ensures that the item list is always in the "correct" order
            //The Graph will always keep an internal consistent index of Nodes based on the order they were added to the graph
            //Whereas if we base the ItemDef indices on a locally stored list, the indexes could change if connections are broken and re-added
            //This would disrupt any previously set-up nodes that reference item indices
            if (graph is ScenarioNodeGraph scenarioGraph)
            {
                return scenarioGraph.GetItemList();
            }

            return null;
        }
    }


    [SerializeField] private int _seed;
    public int Seed => _seed;

    private List<NodePort> _entityPorts = new List<NodePort>();
    public List<NodePort> EntityPorts => _entityPorts;

    public string Name => scenarioName;
    public int TotalRounds => rounds;
    public int ActionsPerRound => actionsPerRound;

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
    }

    private void ClearEntityPorts()
    {
        if ((_entityPorts != null) && (_entityPorts.Count > 0))
        {
            foreach (NodePort port in _entityPorts)
            {
                RemoveDynamicPort(port);
            }

            _entityPorts.Clear();
        }
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        if (port == GetInputPort("_matrix"))
        {
            ClearEntityPorts();
        }
    }

    #region Map
    public MapLayout GetMapLayout()
    {
        return (MapLayout)GetInputPort("_map").GetInputValue();
    }
    #endregion
}
