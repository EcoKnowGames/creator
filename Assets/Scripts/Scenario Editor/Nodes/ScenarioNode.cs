using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

/*public class Scenario
{
	public string name { get; }
	public int rounds { get; }
	public int stepsPerRound { get; }
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
    [SerializeField] protected int stepsPerRound;
    [SerializeField] protected int actionsPerRound;
    [SerializeField] protected int startCurrency;

    [Input(ShowBackingValue.Never)] [SerializeField] private Matrix _matrix;
    [Input(ShowBackingValue.Never)] [SerializeField] private MapLayout _map;

    [SerializeField] private int _seed;

    private List<NodePort> _entityPorts;
    public List<NodePort> EntityPorts => _entityPorts;

    public string Name => scenarioName;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();
        _entityPorts = new List<NodePort>();
    }

    private void OnValidate()
    {
        Debug.Log("VALIDATING SCENARIO");
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        foreach(NodePort entity in _entityPorts)
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
        //TODO(caspar): Tidy this whole thing up

        if (GetInputPort("_matrix").IsConnected)
        {
            Matrix matrix = (Matrix)GetInputPort("_matrix").GetInputValue();
            ClearEntityPorts();

            //if (GetPort("EntityTest") == null)
            {
                foreach (string id in matrix.entityIDs)
                {
                    if (_entityPorts.FirstOrDefault(x => x.fieldName.Equals(id)) == null)
                    {
                        _entityPorts.Add(AddDynamicOutput(typeof(Entity), fieldName: id));
                    }
                }
            }
        }
        else
        {
            //if (GetPort("EntityTest") != null)
            {
                ClearEntityPorts();
            }
        }
    }

    private void ClearEntityPorts()
    {
        foreach (NodePort port in _entityPorts)
        {
            RemoveDynamicPort(port);
        }

        _entityPorts.Clear();
    }


    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        if (port == GetInputPort("_matrix"))
        {
            ClearEntityPorts();
        }
    }

    /*#region Scenario
	public Scenario GetScenario()
    {
		return new Scenario(this);
    }
    #endregion*/

    #region Map
    public MapLayout GetMapLayout()
    {
        return (MapLayout)GetInputPort("_map").GetInputValue();
    }
    #endregion
}
