using System.Collections.Generic;
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

    [SerializeField] private int _startPopulation = 100;

    [SerializeField] private bool _canHarvest;
    public bool CanHarvest { get { return _canHarvest; } set { _canHarvest = value; } }
    [SerializeField] private int _harvestLimit;
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private Quantity _harvestQuantity;

    [SerializeField] private bool _canIntroduce;
    public bool CanIntroduce { get { return _canIntroduce; } set { _canIntroduce = value; } }

    [SerializeField] private int _introduceLimit;
    [Input(ShowBackingValue.Never, ConnectionType.Multiple)] [SerializeField] private Quantity _introduceQuantity;

    [SerializeField] private List<EntityZoneInformation> _zoneInformation = new List<EntityZoneInformation>();
    public List<EntityZoneInformation> ZoneInformation => _zoneInformation;

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
        return new Entity(_id, _iconPath, GetColourFromIndex(), _growthRate, _movementRate, _vulnerable, _abundance, _autoPlace, _startPopulation, _canHarvest, _canIntroduce, _harvestLimit, _introduceLimit, GetHarvestQuantities(), GetIntroduceQuantities(), ZoneInformation.ToArray());
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

    #region Zones
    public void AdjustZoneRates()
    {
        if (!IsConnected())
            return;

        ZoneDef[] zoneDefs = GetZoneDefs();
        if (zoneDefs != null && zoneDefs.Count() > 0)
        {
            int[] zoneIDs = zoneDefs.Select(x => x.ID).ToArray();

            //Add any we're missing
            foreach(int id in zoneIDs)
            {
                if (!_zoneInformation.Any(x => x.ZoneID == id))
                {
                    _zoneInformation.Add(new EntityZoneInformation(id));
                }
            }

            //Remove excess
            _zoneInformation.RemoveAll(x => !zoneIDs.Contains(x.ZoneID));
        }
        else
        {
            //Clear
            _zoneInformation.Clear();
        }
    }

    private ZoneDef[] GetZoneDefs()
    {
        NodePort outputPort = GetInputPort("_id");
        if (outputPort != null && outputPort.IsConnected)
        {
            foreach (var connection in outputPort.GetConnections())
            {
                if (connection.node is ScenarioNode scenarioNode)
                {
                    return scenarioNode.MapLayout?.gridDef?.zoneDefs;
                }
            }
        }
        return null;
    }
    #endregion

    public bool IsConnected()
    {
        NodePort port = GetInputPort("_id");
        if ((port != null) && (port.IsConnected))
        {
            if (port.Connection != null && port.Connection.node is ScenarioNode scenario)
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

        if (to == GetInputPort("_id"))
        {
            _id = GetInputPort("_id").GetInputValue().ToString();
            AdjustZoneRates();
        }
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        if (port == GetInputPort("_id"))
        {
            _id = string.Empty;
        }
    }
}
