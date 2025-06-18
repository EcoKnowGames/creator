using System.Collections;
using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;


public class ItemQuantityNode : BaseQuantityNode
{
    [SerializeField, HideInInspector] protected int itemIndex;
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

    protected override string ID
    {
        get
        {
            if ((AvailableItems != null) && (ItemIndex > 0) && (itemIndex < AvailableItems.Count))
            {
                return AvailableItems[itemIndex].ID;
            }

            return null;
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

    protected override Quantity GetQuantity()
    {
        if ((AvailableItems != null) && (ItemIndex >= 0) && (itemIndex < AvailableItems.Count))
        {
            return new Quantity(AvailableItems[itemIndex].ID, Value);
        }

        return null;
    }
}
