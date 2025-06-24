using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;


public class EntityNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private string _id;
    public string ID => _id;

    [SerializeField] private Sprite _icon;
    public Sprite Icon { get { return _icon; } set { _icon = value; } }

    [SerializeField] private float _growthRate;
    [SerializeField] private float _movementRate;
    [SerializeField] private float _vulnerable;
    [SerializeField] private float _abundance;

    [SerializeField] private bool _canHarvest;
    public bool CanHarvest { get { return _canHarvest; } set { _canHarvest = value; } }
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private Quantity _harvestQuantity;

    [SerializeField] private bool _canIntroduce;
    public bool CanIntroduce { get { return _canIntroduce; } set { _canIntroduce = value; } }
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private Quantity _introduceQuantity;

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
        return new Entity(_id, _icon, _growthRate, _movementRate, _vulnerable, _abundance, _canHarvest, _canIntroduce, GetHarvestQuantities(), GetIntroduceQuantities());
    }

    private Quantity[] GetHarvestQuantities()
    {
        if (GetInputPort("_harvestQuantity").GetConnections().Count > 0)
        {
            return GetInputPort("_harvestQuantity").GetInputValues<Quantity>();
        }

        return null;
    }

    private Quantity[] GetIntroduceQuantities()
    {
        if (GetInputPort("_introduceQuantity").GetConnections().Count > 0)
        {
            return GetInputPort("_introduceQuantity").GetInputValues<Quantity>();
        }

        return null;
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
