using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ScenarioNode))]
public class ScenarioNodeEditor : NodeEditor
{
    private ScenarioNode _scenarioNode;
    private bool _matrixConnected;

    public override void OnBodyGUI()
    {
        if (_scenarioNode == null) _scenarioNode = target as ScenarioNode;

        // Update serialized object's representation
        serializedObject.Update();

        EditorGUIUtility.labelWidth = 125.0f;

        EditorGUILayout.LabelField("Info", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("scenarioName"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("authorName"));
        EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("rounds"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("actionsPerRound"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("startCurrency"));

        EditorGUILayout.Space();
        string supportedPlayers = GetCompatiblePlayers(_scenarioNode.ActionsPerRound);
        EditorGUILayout.HelpBox($"This Scenario will support {supportedPlayers} player(s)", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Entities", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_matrix"));
        foreach (NodePort port in _scenarioNode.EntityPorts)
        {
            NodeEditorGUILayout.PortField(port);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Win Conditions", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_winConditions"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inventory Items", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_items"));
        if (_scenarioNode.ItemDefs != null)
        {
            GUIStyle itemStyle = new GUIStyle();
            itemStyle.richText = true;
            itemStyle.fontStyle = FontStyle.Bold;
            itemStyle.fontSize = 10;

            foreach (Item item in _scenarioNode.ItemDefs)
            {
                bool itemIdInvalid = string.IsNullOrEmpty(item.ID);
                bool itemIdDuplicated = itemIdInvalid ? false : _scenarioNode.ItemDefs.Where(x => !string.IsNullOrEmpty(x.ID) && x.ID.Equals(item.ID)).Count() > 1;

                string displayName = string.Format($"<color=green>- {item.ID}</color>");
                if (itemIdInvalid)
                {
                    displayName = "<color=yellow>- Invalid Item - ID Missing!</color>";
                }
                else if (itemIdDuplicated)
                {
                    displayName = string.Format($"<color=yellow>- Invalid Item - ID [{item.ID}] is Duplicated!</color>");
                }

                EditorGUILayout.LabelField(displayName, itemStyle);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_map"));
        if (!_scenarioNode.DoesMapPopulationCountMatchMatrix())
        {
            int compatibleMapEntities = _scenarioNode.MapLayout != null && _scenarioNode.MapLayout.gridDef != null ? _scenarioNode.MapLayout.gridDef.GetValidEntityCount() : 0;
            int connectedEntities = _scenarioNode.GetConnectedEntityCount();
            EditorGUILayout.HelpBox($"The connected Map is compatible for ({compatibleMapEntities}) Entities, but the number of connected Entity Nodes is ({connectedEntities}). Some Entities may not be initialised on the Map when playing the Scenario", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Seed", EditorStyles.centeredGreyMiniLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_seed"));

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

    private string GetCompatiblePlayers(int maxActions)
    {
        string players = "1";

        if (maxActions % 2 == 0)
        {
            players += ", 2";
        }

        if (maxActions % 3 == 0)
        {
            players += ", 3";
        }

        if (maxActions % 4 == 0)
        {
            players += ", 4";
        }

        //Syntax
        int lastComma = players.LastIndexOf(',');
        if (lastComma >= 0)
        {
            players = players.Remove(lastComma, 1).Insert(lastComma, " or");
        }

        return players;
    }
}
