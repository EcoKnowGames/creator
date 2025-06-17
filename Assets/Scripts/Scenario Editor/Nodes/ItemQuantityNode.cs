using System.Collections;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

public record ItemQuantity
(
    string ItemId,
    int Quantity
);


public class ItemQuantityNode : Node {

    [Output(ShowBackingValue.Always, ConnectionType.Override)] [SerializeField] private ItemQuantity _itemQuantity;

    [SerializeField] private int _quantity;
    public int Quantity => _quantity;

    protected int itemIndex = 0;
    public int ItemIndex
    {
        get
        {
            return itemIndex;
        }
        set
        {
            itemIndex = value;
        }
    }

    public List<Item> AvailableItems
    {
        get
        {
            //Return valid item list from graph
            if (graph is ScenarioNodeGraph scenarioGraph)
            {
                return scenarioGraph.GetItemList();
            }

            return null;
        }
    }

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
            if ((AvailableItems != null) && (ItemIndex > 0) && (itemIndex < AvailableItems.Count))
            {
                return new ItemQuantity(AvailableItems[itemIndex].ID, Quantity);
            }
        }

        return null;
	}
}
