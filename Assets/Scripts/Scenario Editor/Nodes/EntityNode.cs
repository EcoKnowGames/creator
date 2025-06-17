using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;


public class EntityNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private string _id;
    public string ID => _id;

    [SerializeField] private Sprite _icon;

    [SerializeField] private float _growthRate;
    [SerializeField] private float _movementRate;
    [SerializeField] private float _vulnerable;
    [SerializeField] private float _abundance;

    private bool _harvestable;
    public bool Harvestable { get { return _harvestable; } set { _harvestable = value; } }
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private ItemQuantity _harvestQuantity;

    private bool _introducable;
    public bool Introducable { get { return _introducable; } set { _introducable = value; } }
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private ItemQuantity _introduceQuantity;

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

    public Entity GetEntity()
    {
        return new Entity(_id, _growthRate, _movementRate, _vulnerable, _abundance);
    }

    public bool IsConnected()
    {
        NodePort port = GetInputPort("_id");
        if ((port != null) && (port.IsConnected))
        {
            if (port.Connection.node is ScenarioNode scenario)
            {
                return true;
            }    
        }

        return false;
    }

    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);

        _id = GetInputPort("_id").GetInputValue().ToString();
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        _id = string.Empty;
    }
}
