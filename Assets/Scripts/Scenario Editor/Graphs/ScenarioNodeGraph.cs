using System;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using XNode;

public record Scenario
    (
        string Name,
        int Rounds,
        int ActionsPerRound,
        int StartCurrency,
        int Seed,
        Entity[] Entities,
        Item[] Items,
        WinConditionRecord[] WinConditions,
        Matrix Matrix,
        MapLayout Map
    );

[CreateAssetMenu]
public class ScenarioNodeGraph : NodeGraph
{
    [SerializeField] public ColourPaletteObject colourPalette;

    public override Node AddNode(Type type)
    {

#if UNITY_EDITOR
        if (type == typeof(ScenarioNode))
        {
            if (GetScenarioNode() != null)
            {
                Debug.LogError("[NODE EDITOR] ERROR: Trying to add a second Scenario node when one already exists. Aborting node spawn!");
                EditorUtility.DisplayDialog("ERROR", "Trying to add a second Scenario node when one already exists. Aborting node spawn!", "OK");
                return null;
            }
        }
#endif
        return base.AddNode(type);
    }

    public ScenarioNode GetScenarioNode()
    {
        return nodes.OfType<ScenarioNode>().FirstOrDefault();
    }

    public bool HasConnectedEntityNodes()
    {
        return nodes.OfType<EntityNode>().Where(x => x.IsConnected()).ToList().Count > 0;
    }

    public List<Entity> GetEntityList()
    {
        return nodes.OfType<EntityNode>().Where(x => x.IsConnected()).Select(x => x.GetEntity()).ToList();
    }

    #region Colours
    public ColourPaletteObject.ColourSwatch[] GetColours()
    {
        if (colourPalette != null)
        {
            return colourPalette.Colours;
        }

        return null;
    }

    public Color GetColour(int index)
    {
        if ((colourPalette != null) && (colourPalette.Colours != null))
        {
            if ((index >= 0) && (index < colourPalette.Colours.Count()))
            {
                return colourPalette.Colours[index].colour;
            }
        }

        return Color.white;
    }
    #endregion

    #region Import/Export

#if UNITY_EDITOR

    [ContextMenu("Export JSON")]
    public void ExportJson()
    {

        ScenarioNode scenarioNode = GetScenarioNode();
        if (scenarioNode == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no Scenario Node was found on the graph.", "OK");
            return;
        }
        else
        {
            //Collect our data
            Scenario scenario = new Scenario(
               scenarioNode.Name,
               scenarioNode.TotalRounds,
               scenarioNode.ActionsPerRound,
               scenarioNode.StartCurrency,
               scenarioNode.Seed,
               GetEntityList().ToArray(),                //Note(caspar): I wanted to get this from the scenarioNode rather than the graph, but there are issues with the connections on dynamic ports (disconnecting each time code recompiles) that makes this hard to test otherwise
               scenarioNode.ItemDefs.ToArray(),
               scenarioNode.WinConditions.ToArray(),
               scenarioNode.Matrix,
               scenarioNode.MapLayout
               );

            string json = FormatJson(scenario);
            if (!string.IsNullOrEmpty(json))
            {
                string fileName = scenario.Name;
                string filePath = Application.persistentDataPath + "/" + fileName + ".json";

                System.IO.File.WriteAllText(filePath, json);

                EditorUtility.DisplayDialog("Scenario Editor Export", "Scenario has been exported successfully to destination " + filePath, "OK");
            }
        }
    }

    private string FormatJson(Scenario scenario)
    {
        string scenarioJson = JsonConvert.SerializeObject(scenario, Formatting.Indented);
        Debug.Log(scenarioJson);

        return scenarioJson;
    }

#endif

    /*[ContextMenu("Import JSON")]
    public static void ImportJSON()
    {
        string json = "";

        if (!string.IsNullOrEmpty(json))
        {
            ScenarioNodeGraph newScenarioNodeGraph = JsonUtility.FromJson<ScenarioNodeGraph>(json);
        }
    }*/

    #endregion
}
