using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using XNode;
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
        NodePort outputPort = _mapNode.GetOutputPort("map");
        NodeEditorGUILayout.PortField(new GUIContent("Map"), outputPort);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Details", EditorStyles.centeredGreyMiniLabel);

        GUI.enabled = false;

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("map.fileName"));

        EditorGUIUtility.labelWidth = 180;
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("map.gridDef.rows"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("map.gridDef.columns"));

        int validEntityCount = _mapNode.GetValidEntityCount();
        string entityCountStr = validEntityCount >= 0 ? validEntityCount.ToString() : "Not Specified";
        EditorGUILayout.TextField("Compatible # of Entities", entityCountStr);

        GUI.enabled = true;

        // Zone definitions
        ZoneDef[] zoneDefs = _mapNode.map?.gridDef?.zoneDefs;
        if (zoneDefs != null && zoneDefs.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Zones", EditorStyles.centeredGreyMiniLabel);

            foreach (ZoneDef zone in zoneDefs)
            {
                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"{zone.Name} [{zone.ID}]", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100));

                Color zoneColour = Color.white;
                ColorUtility.TryParseHtmlString(zone.Colour, out zoneColour);

                Rect colourRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(colourRect, zoneColour);

                GUILayout.EndHorizontal();

            }
        }

        // Apply property modifications
        serializedObject.ApplyModifiedProperties();
    }

}
