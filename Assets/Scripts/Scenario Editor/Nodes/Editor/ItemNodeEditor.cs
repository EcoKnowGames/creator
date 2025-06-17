using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ItemNode))]

public class ItemNodeEditor : NodeEditor
{
    private ItemNode _itemNode;

    public override void OnBodyGUI()
    {
        if (_itemNode == null)
            _itemNode = target as ItemNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_id"));

        EditorGUILayout.Space();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_icon"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_value"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_canSell"));


        if (!_itemNode.IsConnected())
        {
            EditorGUILayout.HelpBox("Item node is not connected to a Scenario node.", MessageType.Warning);
        }

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}
