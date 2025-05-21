using UnityEngine;
using XNode;

[System.Serializable]
public class Matrix { }


public class MatrixNode : Node
{

    [SerializeField] private TextAsset matricesCSV;

    [Output] public Matrix matrix;


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
}
