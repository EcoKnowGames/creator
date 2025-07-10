using System;
using System.Collections.Generic;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using XNode;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

//All data needed to create a scenario
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

//Header info
public record ScenarioConfig
    (
        string AppVersion,
        string UnityVersion,
        Scenario Scenario
    );

[CreateAssetMenu]
public class ScenarioNodeGraph : NodeGraph
{
    [SerializeField] public ColourPaletteObject colourPalette;

    [SerializeField] private string _appVersion;
    [SerializeField] private string _unityVersion;
    [SerializeField] private string _xNodeVersion;

    public string AppVersion => _appVersion;
    public string UnityVersion => _unityVersion;
    public string XNodeVersion => _xNodeVersion;

    void Reset()
    {
        _appVersion = Application.version;
        _unityVersion = Application.unityVersion;

        PackageInfo info = PackageInfo.FindForPackageName("com.github.siccity.xnode");
        if (info != null)
        {
            _xNodeVersion = info.version;
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        //TODO(caspar): Do we upgrade graphs to the latest version? But not downgrade? How do we resolve this? Do we upgrade when the user saves changes to the object?

        string warningStr = string.Format($"Warning, The Scenario Graph \"{this.name}\" was created using a different version of the app. It may be incompatible and some nodes may not behave as expected.\n");
        bool showWarning = false;

        //Check App
        if (_appVersion != Application.version)
        {
            showWarning = true;
            warningStr += string.Format($"\nApp Version: {_appVersion}    (Current: {Application.version})");
        }

        //Check Unity
        if (_unityVersion != Application.unityVersion)
        {
            showWarning = true;
            warningStr += string.Format($"\nUnity Version: {_unityVersion}    (Current: {Application.unityVersion})");
        }

        //Check xNode
        string xNodeCurrent = "Unknown";
        PackageInfo info = PackageInfo.FindForPackageName("com.github.siccity.xnode");
        if (info != null)
        {
            xNodeCurrent = info.version;
        }

        if (_xNodeVersion != xNodeCurrent)
        {
            showWarning = true;
            warningStr += string.Format($"\nxNode Version: {_xNodeVersion}    (Current: {xNodeCurrent})");
        }

        if (showWarning)
        {
            EditorUtility.DisplayDialog("ERROR", warningStr, "OK");
        }
#endif
    }

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

    public ScenarioConfig GetConfig()
    {
        ScenarioNode scenarioNode = GetScenarioNode();
        if (scenarioNode == null)
        {
            return null;
        }

        //Collect our data
        Scenario scenario = new Scenario(
           scenarioNode.Name,
           scenarioNode.TotalRounds,
           scenarioNode.ActionsPerRound,
           scenarioNode.StartCurrency,
           scenarioNode.Seed,
           GetEntityList().ToArray(),                //Note(caspar): I wanted to get this from the scenarioNode rather than the graph, but there are issues with the connections on dynamic ports (disconnecting each time code recompiles) that makes this hard to test otherwise
           scenarioNode.ItemDefs == null ? null : scenarioNode.ItemDefs.ToArray(),
           scenarioNode.WinConditions.ToArray(),
           scenarioNode.Matrix,
           scenarioNode.MapLayout
           );

        //Package it up with any additional header data we might need
        ScenarioConfig config = new ScenarioConfig(
            Application.version,
            Application.unityVersion,
            scenario
            );

        return config;
    }

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
            //Validation
            if (string.IsNullOrEmpty(scenarioNode.Name))
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no name was defined for the Scenario!", "OK");
                return;
            }

            if (scenarioNode.MapLayout == null)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no map was defined for the Scenario. Make sure a Map Node exists and is connected to the Scenario Node.", "OK");
                return;
            }

            if (scenarioNode.Matrix == null)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no matrix was defined for the Scenario. Make sure a Matrix Node exists and is connected to the Scenario Node.", "OK");
                return;
            }

            if (GetEntityList().Count <= 0)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no entities were defined for the Scenario. Make sure Entity Nodes exist and are connected to the Scenario Node.", "OK");
                return;
            }

            if (scenarioNode.Matrix.entityIDs.Count() != GetEntityList().Count)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, the number of connected Entity Nodes does not match the number of Entities defined by the Matrix. Make sure all required Entity Nodes exist and are connected to the Scenario Node.", "OK");
                return;
            }

            if (scenarioNode.WinConditions == null || scenarioNode.WinConditions.Count() <= 0)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, no win conditions have been defined for the Scenario. Make sure at least one Win Condition Node exists and is connected to the Scenario Node.", "OK");
                return;
            }

            ScenarioConfig config = GetConfig();

            if (config == null)
            {
                EditorUtility.DisplayDialog("ERROR", "Scenario export failed, ScenarioConfig did not serialise correctly. Please check the Node setup and try again", "OK");
            }
            else
            {
                //Sort out filepath and export
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                if (!string.IsNullOrEmpty(json))
                {
                    string fileName = config.Scenario.Name;
                    string filePath = Application.persistentDataPath + "/" + fileName + ".json";

                    System.IO.File.WriteAllText(filePath, json);

                    EditorUtility.DisplayDialog("Scenario Editor Export", "Scenario has been exported successfully to destination " + filePath, "OK");
                }
            }
        }
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
