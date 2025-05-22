using UnityEngine;
using XNode;

[System.Serializable]
public class Entity
{
    public string id;
}

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

        //_id = GetInputPort("_id").GetInputValue().ToString();
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return null; // Replace this
    }

    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);

        _id = GetInputPort("_id").GetInputValue().ToString();

        //if (to)
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        _id = string.Empty;
    }
}
