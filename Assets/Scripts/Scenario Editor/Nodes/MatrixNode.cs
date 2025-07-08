using System;
using Newtonsoft.Json;
using UnityEngine;
using XNode;


[System.Serializable]
public class Matrix
{
    public string[] entityIDs;
    public float[,] entityMatrix;

    [JsonConstructor]
    public Matrix() { }

    public Matrix(TextAsset matrixCSV)
    {
        string rawCSV = matrixCSV.text;
        rawCSV = rawCSV.Trim(' ', '\n', '\r');

        string[] values = rawCSV.Replace("\r", string.Empty).Replace("\n", ",").Split(',');

        int rows = rawCSV.Split('\n').Length;
        int columns = values.Length / rows;

        if (columns != (rows - 1))
        {
            //Error
            Debug.LogError($"Error reading matrix CSV [{matrixCSV.name}]! Row and Column count do not match.");
        }

        entityMatrix = new float[columns, rows - 1];
        entityIDs = new string[columns];
        Array.Copy(values, entityIDs, columns);

        int index = columns;
        for (int y = 1; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float value = 0f;
                float.TryParse(values[index], out value);
                entityMatrix[x, y - 1] = value;
                index++;
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

            if (GetOutputPort("matrix").IsConnected)
            {
                if (GetOutputPort("matrix").Connection.node is ScenarioNode scenario)
                {
                    scenario.ProcessMatrix();
                }
            }
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

    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);

        if (to.node is ScenarioNode scenario)
        {
            scenario.ProcessMatrix();
        }
    }

    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);

        Debug.Log(port.node.GetType());
    }
}
