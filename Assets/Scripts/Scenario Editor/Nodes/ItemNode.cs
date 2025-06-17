using Glitchers.EcoKnow.Sandbox;
using UnityEngine;
using XNode;

public class ItemNode : Node
{

    [Output(ShowBackingValue.Always, ConnectionType.Override)] [SerializeField] private string _id;
    public string ID => _id;

    [SerializeField] private Sprite _icon;


    [SerializeField] private int _value;
    [SerializeField] private bool _canSell;



    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return null; // Replace this
    }

    public Item GetItem()
    {
        return new Item(_id, _icon, _value, _canSell);
    }

    public bool IsConnected()
    {
        NodePort port = GetOutputPort("_id");
        if ((port != null) && (port.IsConnected))
        {
            if (port.Connection.node is ScenarioNode scenario)
            {
                return true;
            }
        }

        return false;
    }
}
