using Glitchers.EcoKnow.Sandbox.Grid;
using Newtonsoft.Json;
using UnityEngine;
using XNode;
using static XNode.Node;

[System.Serializable]
public class MapLayout
{
    //TODO
    public string fileName;
    public GridDef gridDef;

    [JsonConstructor]
    public MapLayout() { }
    public MapLayout(TextAsset csv)
    {
        fileName = csv.name;
        gridDef = GridManager.LoadGridDef(csv);
    }
}

[NodeWidth(300)]
public class MapNode : Node
{
    [SerializeField] private TextAsset mapCSV;

    [Output(ShowBackingValue.Always)] public MapLayout map;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();
    }

    private void OnValidate()
    {
        if (mapCSV != null)
        {
            map = new MapLayout(mapCSV);
        }
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "map")
        {
            return map;
        }
        else
        {
            return null;
        }
    }

    public int GetValidEntityCount()
    {
        if ((map != null) && (map.gridDef != null))
        {
            return map.gridDef.GetValidEntityCount();
        }

        return -1;
    }
}

