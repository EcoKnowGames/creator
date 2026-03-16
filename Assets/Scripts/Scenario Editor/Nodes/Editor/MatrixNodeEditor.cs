using System.Linq;
using System.Security.Policy;
using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(MatrixNode))]
public class MatrixNodeEditor : NodeEditor
{
    private MatrixNode _matrixNode;

    public override void OnBodyGUI()
    {
        if (_matrixNode == null) _matrixNode = target as MatrixNode;

        serializedObject.Update();

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("matricesCSV"));

        if (_matrixNode.IsConnected() && !_matrixNode.IsCsvValid())
        {
            EditorGUILayout.HelpBox("Please provide a valid CSV file", MessageType.Warning);
            EditorGUILayout.Space();
        }

        // Zone dropdown
        ZoneDef[] zoneDefs = GetZoneDefs();
        if (zoneDefs != null && zoneDefs.Length > 1)
        {
            int zoneIndex = Mathf.Clamp(_matrixNode.ZoneIndex, 0, zoneDefs.Length - 1);
            ZoneDef selectedZone = zoneDefs[zoneIndex];

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Zone", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(EditorGUIUtility.labelWidth));

            Color zoneColour = Color.white;
            ColorUtility.TryParseHtmlString(selectedZone.Colour, out zoneColour);

            Rect colourRect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(colourRect, zoneColour);

            Rect popupRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
            zoneIndex = EditorGUI.Popup(popupRect, zoneIndex, zoneDefs.OrderBy(x => x.ID).Select(x => $"{x.Name} [{x.ID}]").ToArray());

            GUILayout.EndHorizontal();

            _matrixNode.ZoneIndex = zoneIndex;

            EditorGUILayout.Space();

            if (IsZoneDefInUse(_matrixNode.ZoneIndex))
            {
                EditorGUILayout.HelpBox("Zone already in use on another Matrix node. This will result in an invalid scenario.", MessageType.Warning);
            }
        }

        // Matrix output port
        NodePort outputPort = _matrixNode.GetOutputPort("matrix");
        NodeEditorGUILayout.PortField(new GUIContent("Matrix"), outputPort);

        serializedObject.ApplyModifiedProperties();
    }

    private ZoneDef[] GetZoneDefs()
    {
        // Walk: MatrixNode -> ScenarioNode -> MapLayout -> GridDef -> zoneDefs
        NodePort outputPort = _matrixNode.GetOutputPort("matrix");
        if (outputPort != null && outputPort.IsConnected)
        {
            foreach (var connection in outputPort.GetConnections())
            {
                if (connection.node is ScenarioNode scenarioNode)
                {
                    return scenarioNode.MapLayout?.gridDef?.zoneDefs;
                }
            }
        }
        return null;
    }

    private bool IsZoneDefInUse(int id)
    {
        if (_matrixNode.graph is ScenarioNodeGraph scenarioGraph)
        {
            return scenarioGraph.nodes.OfType<MatrixNode>().Where(x => x != _matrixNode && x.IsConnected()).Any(x => x.ZoneIndex == id);
        }

        return false;
    }
}
