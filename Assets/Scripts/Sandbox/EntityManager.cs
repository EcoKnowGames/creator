using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public record Entity(
    string ID,
    float GrowthRate,
    float MovementRate
    );


public class EntityManager : MonoBehaviour
{
    private Dictionary<string, Entity> _entityDictionary;
    public List<Entity> AllEntities => _entityDictionary == null ? null : _entityDictionary.Select(x => x.Value).ToList();

    public void RegisterEntities(List<Entity> entityRecords)
    {
        if ((entityRecords == null) || (entityRecords.Count <= 0))
        {
            return;
        }

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
