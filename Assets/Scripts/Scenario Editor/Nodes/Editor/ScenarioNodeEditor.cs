using UnityEditor;
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

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("scenarioName"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("rounds"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("actionsPerRound"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("startCurrency"));

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_matrix"));
        foreach (NodePort port in _scenarioNode.EntityPorts)
        {
            NodeEditorGUILayout.PortField(port);
        }

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_map"));

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_seed"));



        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }
}
