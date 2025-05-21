using UnityEngine;
using XNode;

[System.Serializable]
public class Entity
{
    public string id;
}

[System.Serializable]
public class Matrix
{
    public string[] entityIDs;
    public string[,] entityMatrix;

    public Matrix(TextAsset matrixCSV)
    {
        string rawCSV = matrixCSV.text;
        rawCSV = rawCSV.Trim(' ', '\n', '\r');

        string[] IDs = rawCSV.Replace("\r", string.Empty).Replace("\n", ",").Split(',');

        int rows = rawCSV.Split('\n').Length;
        int columns = IDs.Length / rows;

        entityMatrix = new string[columns, rows];
        entityIDs = new string[columns];

        int tileIndex = 0;
        int entityIndex = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                entityMatrix[x, y] = IDs[tileIndex];
                tileIndex++;

                if (y == 0)
                {
                    entityIDs[x] = IDs[entityIndex];
                    entityIndex++;
                }
            }
        }

        Debug.Log($"Matrix: {matrixCSV.name} / Entities: {entityIDs.Length}");
    }
}


public class MatrixNode : Node
{

    [SerializeField] private TextAsset matricesCSV;

    [Output(ShowBackingValue.Always)] public Matrix matrix;


    // Use this for initialization
    protected override void Init()
    {
        base.Init();

    }

    private void OnValidate()
    {
        if (matricesCSV != null)
        {
            matrix = new Matrix(matricesCSV);
        }
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "matrix")
        {
            return matrix;
        }
        else
        {
            return null;
        }
    }
}
