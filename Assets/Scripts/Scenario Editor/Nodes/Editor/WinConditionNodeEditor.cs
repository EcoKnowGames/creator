using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(WinConditionNode))]
public class WinConditionNodeEditor : NodeEditor
{
    private WinConditionNode _winConditionNode;
    public int entityIndex = 0;

    public override void OnBodyGUI()
    {
        if (_winConditionNode == null)
            _winConditionNode = target as WinConditionNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodePort outputPort = _winConditionNode.GetOutputPort("_id");
        NodeEditorGUILayout.PortField(outputPort);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Description", EditorStyles.centeredGreyMiniLabel);

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);

        //Entity list
        if ((_winConditionNode.AvailableEntities != null) && (_winConditionNode.AvailableEntities.Count > 0))
        {
            entityIndex = EditorGUILayout.Popup("Entity", entityIndex, _winConditionNode.AvailableEntities.Select(x => x.ID).ToArray());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Population Range", EditorStyles.miniBoldLabel);

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("minValue"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("maxValue"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rounds", EditorStyles.miniBoldLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("requiredRounds"), new GUIContent("Rounds"));

        }
        else
        {
            EditorGUILayout.HelpBox("No Entities found. Please connect to a Scenario Node which has valid, connected Entity Nodes.", MessageType.Warning);
        }

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}
