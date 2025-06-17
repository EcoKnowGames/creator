using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(CurrencyQuantityNode))]
public class CurrencyQuantityNodeEditor : NodeEditor
{
    private CurrencyQuantityNode _currencyQuantityNode;

    public override void OnBodyGUI()
    {
        if (_currencyQuantityNode == null)
            _currencyQuantityNode = target as CurrencyQuantityNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodePort outputPort = _currencyQuantityNode.GetOutputPort("_quantity");
        NodeEditorGUILayout.PortField(new GUIContent("Quantity"), outputPort);
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_value"), new GUIContent("Quantity"));

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}
