using System;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class ScenarioNodeGraph : NodeGraph
{
    public override Node AddNode(Type type)
    {
        if (type == typeof(ScenarioNode))
        {
            if (GetScenarioNode() != null)
            {
                Debug.LogError("[NODE EDITOR] ERROR: Trying to add a second Scenario node when one already exists. Aborting node spawn!");
                EditorUtility.DisplayDialog("Scenario Editor: Error", "Trying to add a second Scenario node when one already exists. Aborting node spawn!", "OK");
                return null;
            }
        }

        return base.AddNode(type);
    }

    public ScenarioNode GetScenarioNode()
    {
        return nodes.OfType<ScenarioNode>().FirstOrDefault();
    }

    public MatrixNode GetMatrixNode()
    {
        return nodes.OfType<MatrixNode>().FirstOrDefault();
    }
    public bool HasConnectedEntityNodes()
    {
        return nodes.OfType<EntityNode>().Where(x => x.IsConnected()).ToList().Count > 0;
    }

    public List<Entity> GetEntityList()
    {
        return nodes.OfType<EntityNode>().Where(x => x.IsConnected()).Select(x => x.GetEntity()).ToList();
    }

    public bool HasConnectedItemNodes()
    {
        return nodes.OfType<ItemNode>().Where(x => x.IsConnected()).ToList().Count > 0;
    }

    public List<Item> GetItemList()
    {
        return nodes.OfType<ItemNode>().Where(x => x.IsConnected()).Select(x => x.GetItem()).OrderBy(x => x.ID).ToList();
    }

    public bool HasConnectedWinConditions()
    {
        return nodes.OfType<WinConditionNode>().Where(x => x.IsConnected()).ToList().Count > 0;
    }

    public List<WinConditionRecord> GetWinConditionList()
    {
        return nodes.OfType<WinConditionNode>().Where(x => x.IsConnected()).Select(x => x.GetWinCondition()).ToList();
    }
}
