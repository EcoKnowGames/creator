using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using XNode;

public class EntityNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] [SerializeField] private string _id;
    public string ID => _id;

    [SerializeField] private Sprite _icon;
    private string _iconPath
    {
        get
        {
#if UNITY_EDITOR
            if (_icon != null)
            {
                string path = AssetDatabase.GetAssetPath(_icon);
                int resourcesIndex = path.IndexOf("Resources/") + "Resources/".Length;
                int extensionIndex = path.LastIndexOf(".");
                if (resourcesIndex >= 0)
                {
                    path = path.Substring(resourcesIndex, extensionIndex - resourcesIndex);
                    return path;
                }
            }
#endif
            return null;
        }
    }

    public Sprite Icon { get { return _icon; } set { _icon = value; } }

    [SerializeField] private int _colourIndex = -1;
    public int ColourIndex { get { return _colourIndex; } set { _colourIndex = value; } }


    [SerializeField] private float _growthRate;
    [SerializeField] private float _movementRate;
    [SerializeField] private float _vulnerable;
    [SerializeField] private float _abundance;

    [SerializeField] private bool _autoPlace;
    public bool AutoPlace { get { return _autoPlace; } set { _autoPlace = value; } }

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

        //We may not have been added to the graph yet
        if (graph is ScenarioNodeGraph scenario)
        {
            if (_colourIndex < 0)
            {
                int colour = scenario.nodes.OfType<EntityNode>().Count();
                _colourIndex = Mathf.Clamp(colour, 0, scenario.GetColours().Length);
            }
        }
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return null;
    }

    public Entity GetEntity()
    {
        return new Entity(_id, _iconPath, GetColourFromIndex(), _growthRate, _movementRate, _vulnerable, _abundance, _autoPlace, _canHarvest, _canIntroduce, GetHarvestQuantities(), GetIntroduceQuantities());
    }

    private string GetColourFromIndex()
    {
        if (graph is ScenarioNodeGraph scenario)
        {
            return ColorUtility.ToHtmlStringRGBA(scenario.GetColour(_colourIndex));
        }

        return ColorUtility.ToHtmlStringRGBA(Color.white);
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

    public bool CanAutoPlace()
    {
        if (IsConnected())
        {
            if (graph is ScenarioNodeGraph scenarioGraph)
            {
                ScenarioNode scenarioNode = scenarioGraph.GetScenarioNode();
                if ((scenarioNode != null) && (scenarioNode.MapLayout != null) && (scenarioNode.MapLayout.gridDef != null))
                {
                    return !scenarioNode.MapLayout.gridDef.HasPopulations();
                }
            }
        }

        return true;
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
