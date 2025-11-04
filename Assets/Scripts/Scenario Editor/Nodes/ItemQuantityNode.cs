using System.Collections.Generic;
using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;


public class ItemQuantityNode : BaseQuantityNode
{
    public int ItemIndex
    {
        get
        {
            if (AvailableItems != null)
            {
                return Mathf.Max(0, AvailableItems.FindIndex(x => x.ID == ID));
            }

            return 0;
        }
        set
        {
            int index = (value >= 0) && (value < AvailableItems.Count) ? value : 0;
            if (AvailableItems != null)
            {
                _selectedID = AvailableItems[index].ID;
            }
        }
    }

    [SerializeField, HideInInspector] protected string _selectedID;
    protected override string ID => _selectedID;

    public List<Item> AvailableItems
    {
        get
        {
            //Return valid item list from graph
            if (graph is ScenarioNodeGraph scenarioGraph)
            {
                return scenarioGraph.GetScenarioNode()?.ItemDefs;
            }

            return null;
        }
    }

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

        if (string.IsNullOrEmpty(_selectedID))
        {
            ItemIndex = 0;
        }
    }

    protected override Quantity GetQuantity()
    {
        if ((AvailableItems != null) && (ItemIndex >= 0) && (ItemIndex < AvailableItems.Count))
        {
            return new Quantity(AvailableItems[ItemIndex].ID, Value);
        }

        return null;
    }
}
