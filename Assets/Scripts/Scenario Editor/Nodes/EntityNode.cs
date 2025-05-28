using UnityEngine;
using XNode;


public class EntityNode : Node
{
    [Input(ShowBackingValue.Always, ConnectionType.Override)] [SerializeField] private string _id;

    [SerializeField] private Sprite _icon;

    [SerializeField] private float _growthRate;
    [SerializeField] private float _movementRate;

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
        return new Entity(_id, _growthRate, _movementRate);
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
