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

    public string Name => scenarioName;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return null;
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
