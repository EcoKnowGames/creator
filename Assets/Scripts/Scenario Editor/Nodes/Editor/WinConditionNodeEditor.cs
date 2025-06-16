using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(WinConditionNode))]
public class WinConditionNodeEditor : NodeEditor
{
    private WinConditionNode _winConditionNode;

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
        int entityIndex = _winConditionNode.EntityIndex;
        if ((_winConditionNode.AvailableEntities != null) && (_winConditionNode.AvailableEntities.Count > 0))
        {
            entityIndex = EditorGUILayout.Popup("Entity", entityIndex, _winConditionNode.AvailableEntities.Select(x => x.ID).ToArray());
            _winConditionNode.EntityIndex = entityIndex;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Population Range", EditorStyles.miniBoldLabel);

            if (!_winConditionNode.IsPopulationRangeValid())
            {
                EditorGUILayout.HelpBox("Error: Minimum population range is greater than the maximum population range. If you do not neet an upper limit, set it to zero or below.", MessageType.Error);
                EditorGUILayout.Space();
            }

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("lowerLimit"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("upperLimit"));

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
