using XNodeEditor;

[CustomNodeEditor(typeof(MapNode))]
public class MapNodeEditor : NodeEditor
{
    private MapNode _mapNode;

    public override void OnBodyGUI()
    {
        if (_mapNode == null) _mapNode = target as MapNode;

        // Update serialized object's representation
        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("mapCSV"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("map"));

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}
