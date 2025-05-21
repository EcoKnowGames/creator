using UnityEngine;
using XNode;


[System.Serializable]
public class MapLayout
{
    //TODO
    public string fileName;

    public MapLayout(string name)
    {
        fileName = name;
    }
}


public class MapNode : Node
{
    [SerializeField] private TextAsset mapCSV;

    [Output] public MapLayout map;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return new MapLayout("wilderness map layout"); // Replace this
    }
}