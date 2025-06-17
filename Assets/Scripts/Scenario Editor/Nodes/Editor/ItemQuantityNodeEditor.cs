using System.Linq;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ItemQuantityNode))]
public class ItemQuantityNodeEditor : NodeEditor
{
    private ItemQuantityNode _itemQuantityNode;

    public override void OnBodyGUI()
    {
        if (_itemQuantityNode == null)
            _itemQuantityNode = target as ItemQuantityNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodePort outputPort = _itemQuantityNode.GetOutputPort("_itemQuantity");
        NodeEditorGUILayout.PortField(outputPort);

        EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);

        //Entity list
        int entityIndex = _itemQuantityNode.ItemIndex;
        if ((_itemQuantityNode.AvailableItems != null) && (_itemQuantityNode.AvailableItems.Count > 0))
        {
            entityIndex = EditorGUILayout.Popup("Item", entityIndex, _itemQuantityNode.AvailableItems.Select(x => x.ID).ToArray());
            _itemQuantityNode.ItemIndex = entityIndex;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_quantity"));
        }
        else
        {
            EditorGUILayout.HelpBox("No Items found. Please connect to a Scenario Node which has valid, connected Item Nodes.", MessageType.Warning);
        }

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}
