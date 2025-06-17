using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public record Quantity
(
    string ID,
    int Value
);


public abstract class BaseQuantityNode : Node
{
    [Output(ShowBackingValue.Always, ConnectionType.Override)] [SerializeField] private Quantity _quantity;

    protected abstract string ID { get; }
    [SerializeField] private int _value;
    public int Value => _value;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    protected virtual Quantity GetQuantity()
    {
        return new Quantity(ID, Value);
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (port == GetOutputPort("_quantity"))
        {
            return GetQuantity();
        }

        return null;
    }
}
