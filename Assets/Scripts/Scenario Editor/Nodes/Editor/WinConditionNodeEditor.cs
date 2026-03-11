using System;
using System.Linq;
using Glitchers.EcoKnow.Sandbox;
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

        EditorStyles.textField.wordWrap = true;

        NodePort outputPort = _winConditionNode.GetOutputPort("_id");
        NodeEditorGUILayout.PortField(outputPort);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Description", EditorStyles.centeredGreyMiniLabel);

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);

        int typeIndex = _winConditionNode.TypeIndex;
        typeIndex = EditorGUILayout.Popup("Type", typeIndex, Enum.GetNames(typeof(WinCondition.TargetType)));
        _winConditionNode.TypeIndex = typeIndex;

        switch((WinCondition.TargetType)_winConditionNode.TypeIndex)
        {
            case (WinCondition.TargetType.Entity):
                {
                    ShowEntityList();
                    break;
                }
            case (WinCondition.TargetType.Item):
                {
                    ShowItemList();
                    break;
                }
            case (WinCondition.TargetType.Currency):
                {
                    ShowCurrency();
                    break;
                }
            default:
                {
                    break;
                }
        }

        if (!_winConditionNode.IsRangeValid())
        {
            EditorGUILayout.HelpBox("Error: Lower Limit is greater than the Upper Limit. If you do not need an Upper Limit, set it to zero or below.", MessageType.Error);
            EditorGUILayout.Space();
        }

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("lowerLimit"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("upperLimit"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rounds", EditorStyles.miniBoldLabel);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("requiredRounds"), new GUIContent("Rounds"));

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

    private void ShowEntityList()
    {
        int entityIndex = _winConditionNode.TargetIndex;
        if ((_winConditionNode.AvailableEntities != null) && (_winConditionNode.AvailableEntities.Count > 0))
        {
            entityIndex = EditorGUILayout.Popup("Entity", entityIndex, _winConditionNode.AvailableEntities.Select(x => x.ID).ToArray());
            _winConditionNode.TargetIndex = entityIndex;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Population Range", EditorStyles.miniBoldLabel);
        }
        else
        {
            EditorGUILayout.HelpBox("No Entities found. Please connect to a Scenario Node which has valid, connected Entity Nodes.", MessageType.Warning);
        }
    }

    private void ShowItemList()
    {
        int itemIndex = _winConditionNode.TargetIndex;
        if ((_winConditionNode.AvailableItems != null) && (_winConditionNode.AvailableItems.Count > 0))
        {
            itemIndex = EditorGUILayout.Popup("Item", itemIndex, _winConditionNode.AvailableItems.Select(x => x.ID).ToArray());
            _winConditionNode.TargetIndex = itemIndex;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quantity Range", EditorStyles.miniBoldLabel);
        }
        else
        {
            EditorGUILayout.HelpBox("No Items found. Please connect to a Scenario Node which has valid, connected Item Nodes.", MessageType.Warning);
        }
    }

    private void ShowCurrency()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quantity Range", EditorStyles.miniBoldLabel);
        _winConditionNode.TargetIndex = 0;
    }
}
