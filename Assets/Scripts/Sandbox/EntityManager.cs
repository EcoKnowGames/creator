using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public record Entity(
    string ID,
    float GrowthRate,
    float MovementRate
    );

/*public record Matrix(
    string[] EntityIDs,
    float[,] EntityAlphas 
    );*/

public class EntityManager : MonoBehaviour
{
    //private float[,] _alphaMatrix;
    private Matrix _entityMatrix;
    public float[,] AlphaMatrix => _entityMatrix.entityMatrix;
    private Dictionary<string, Entity> _entityDictionary;
    public List<Entity> AllEntities => _entityDictionary == null ? null : _entityDictionary.Select(x => x.Value).ToList();

    public void RegisterAlphaMatrix(Matrix matrix)
    {
        _entityMatrix = matrix;
    }

    public void RegisterEntities(List<Entity> entityRecords)
    {
        if ((entityRecords == null) || (entityRecords.Count <= 0) || _entityMatrix == null)
        {
            return;
        }

        //Make sure the order we register matches the order from the alpha matrix. This will be important for maths later
        entityRecords = entityRecords.OrderBy(x => Array.IndexOf(_entityMatrix.entityIDs, x.ID)).ToList();

        _entityDictionary = new Dictionary<string, Entity>();

        foreach (Entity record in entityRecords)
        {
            _entityDictionary.Add(record.ID, record);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
