using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

public class WinConditionNode : Node
{
    [Output(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private string _id;


    [SerializeField] protected string title;
    [SerializeField, Multiline]
    protected string description;

    [SerializeField] protected float minValue;
    [SerializeField] protected float maxValue;

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
}
