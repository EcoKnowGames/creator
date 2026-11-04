using System;
using System.Collections.Generic;
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

        int typeIndex = EditorGUILayout.Popup("Type", _winConditionNode.TypeIndex, Enum.GetNames(typeof(WinCondition.TargetType)));
        if (typeIndex != _winConditionNode.TypeIndex)
        {
            Undo.RecordObject(_winConditionNode, "Change Win Condition Type");
            _winConditionNode.TypeIndex = typeIndex;

            //Clear the target so the legacy index of the previous type can't be resurrected by the lazy migration
            bool isCurrency = (WinCondition.TargetType)typeIndex == WinCondition.TargetType.Currency;
            _winConditionNode.TargetId = isCurrency ? PlayerInventory.CurrencyID : null;
            _winConditionNode.TargetIndex = isCurrency ? 0 : -1;
            EditorUtility.SetDirty(_winConditionNode);
        }

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
        List<Entity> entities = _winConditionNode.AvailableEntities;
        if ((entities != null) && (entities.Count > 0))
        {
            ShowTargetPopup("Entity", entities.Select(x => x.ID).ToArray());

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
        List<Item> items = _winConditionNode.AvailableItems;
        if ((items != null) && (items.Count > 0))
        {
            ShowTargetPopup("Item", items.Select(x => x.ID).ToArray());

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
    }

    //Selects by ID (the authoritative target); the index is only the popup position
    private void ShowTargetPopup(string label, string[] ids)
    {
        string targetId = _winConditionNode.TargetId;
        int selectedIndex = string.IsNullOrEmpty(targetId) ? -1 : Array.FindIndex(ids, x => string.Equals(x, targetId, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(targetId) && (selectedIndex < 0))
        {
            EditorGUILayout.HelpBox($"Target {label} [{targetId}] was not found. Please select a new target.", MessageType.Warning);
        }

        int newIndex = EditorGUILayout.Popup(label, selectedIndex, ids);
        if ((newIndex != selectedIndex) && (newIndex >= 0) && (newIndex < ids.Length))
        {
            Undo.RecordObject(_winConditionNode, "Change Win Condition Target");
            _winConditionNode.TargetId = ids[newIndex];
            _winConditionNode.TargetIndex = newIndex;
            EditorUtility.SetDirty(_winConditionNode);
        }
    }
}
