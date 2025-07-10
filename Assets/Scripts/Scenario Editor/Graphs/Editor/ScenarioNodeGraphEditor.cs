using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomEditor(typeof(ScenarioNodeGraph))]
public class ScenarioNodeGraphEditor : Editor
{
    private ScenarioNodeGraph _scenarioNodeGraph;

    public override void OnInspectorGUI()
    {
        if (_scenarioNodeGraph == null)
            _scenarioNodeGraph = target as ScenarioNodeGraph;

        serializedObject.Update();

        if (GUILayout.Button("Edit graph", GUILayout.Height(40)))
        {
            NodeEditorWindow.Open(serializedObject.targetObject as XNode.NodeGraph);
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("App: " + _scenarioNodeGraph.AppVersion);
        EditorGUILayout.LabelField("Unity: " + _scenarioNodeGraph.UnityVersion);
        EditorGUILayout.LabelField("xNode: " + _scenarioNodeGraph.XNodeVersion);

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("colourPalette"));

        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        GUILayout.Label("Raw data", "BoldLabel");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("nodes"));


        serializedObject.ApplyModifiedProperties();
    }
}
