using UnityEngine;
using XNode;

public class CurrencyQuantityNode : Node
{
    [Output(ShowBackingValue.Always, ConnectionType.Override)] [SerializeField] private ItemQuantity _itemQuantity;

    [SerializeField] private int _quantity;
    public int Quantity => _quantity;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (port == GetOutputPort("_itemQuantity"))
        {
            return new ItemQuantity(Glitchers.EcoKnow.Sandbox.PlayerInventory.CurrencyID, Quantity);
        }

        return null;
    }
}
