using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// Repairs Entity connections that are recorded only on the Entity side: the Entity node's
/// "_id" input lists a Scenario entity port, but that Scenario port's connections don't list
/// the Entity. This state was left on disk by the old Scenario entity port rebuild (#78).
/// xNode only restores the input side from the output side, so these links never heal on
/// their own. Each one is re-made through NodePort.Connect so both sides agree again.
/// Changes are marked dirty but not saved, so the result can be reviewed before saving.
/// </summary>
public static class ScenarioGraphRepair
{
    private const string LogPrefix = "[ScenarioGraphRepair]";

    [MenuItem("Tools/Scenario Editor/Repair Entity Connections")]
    private static void RepairEntityConnections()
    {
        ScenarioNodeGraph graph = Selection.activeObject as ScenarioNodeGraph;
        if (graph == null && NodeEditorWindow.current != null)
        {
            graph = NodeEditorWindow.current.graph as ScenarioNodeGraph;
        }

        if (graph == null)
        {
            Debug.LogWarning($"{LogPrefix} Select a Scenario graph asset (or open one in the Node Editor) and try again.");
            return;
        }

        ScenarioNode scenario = graph.GetScenarioNode();
        if (scenario == null)
        {
            Debug.LogWarning($"{LogPrefix} Graph \"{graph.name}\" has no Scenario node. Nothing to repair.", graph);
            return;
        }

        // Make sure the entity ports and their cache are current before reconnecting
        scenario.ProcessMatrix();

        Undo.SetCurrentGroupName("Repair Entity Connections");
        int undoGroup = Undo.GetCurrentGroup();

        int repairedCount = 0;
        List<string> skipped = new List<string>();
        List<Object> changedObjects = new List<Object>();

        foreach (EntityNode entity in graph.nodes.OfType<EntityNode>().ToList())
        {
            NodePort entityPort = entity.GetInputPort("_id");
            if (entityPort == null)
            {
                skipped.Add($"\"{entity.name}\" (no ID port)");
                continue;
            }

            NodePort target = entityPort.Connection;
            if (target == null)
            {
                skipped.Add($"\"{entity.name}\" (not connected)");
                continue;
            }

            if (target.node != scenario)
            {
                skipped.Add($"\"{entity.name}\" (connected to another node)");
                continue;
            }

            if (target.IsConnectedTo(entityPort))
            {
                // Already consistent on both sides
                continue;
            }

            // The Scenario entity ports are Override ports: connecting would disconnect
            // (and blank the ID of) whichever Entity is currently connected on the Scenario side.
            if (target.ConnectionCount > 0)
            {
                skipped.Add($"\"{entity.name}\" (Scenario port \"{target.fieldName}\" is already connected to another node)");
                continue;
            }

            // ScenarioNode.GetValue only answers for cached entity ports, and
            // EntityNode.OnCreateConnection reads the ID through it.
            if (!scenario.EntityPorts.Contains(target))
            {
                scenario.RefreshEntityPortCache();
            }

            if (!scenario.EntityPorts.Contains(target))
            {
                skipped.Add($"\"{entity.name}\" (Scenario port \"{target.fieldName}\" is not an entity port)");
                continue;
            }

            string idBefore = entity.ID;
            JObject dataBefore = GetEntityData(entity);

            target.Connect(entityPort);

            if (entity.ID != idBefore)
            {
                Debug.LogError($"{LogPrefix} Entity \"{entity.name}\" ID changed from \"{idBefore}\" to \"{entity.ID}\" while repairing graph \"{graph.name}\". Check this entity before saving.", entity);
            }
            else if (!JToken.DeepEquals(dataBefore, GetEntityData(entity)))
            {
                Debug.LogWarning($"{LogPrefix} Entity \"{entity.name}\" ({entity.ID}) had other data changed while repairing graph \"{graph.name}\" (e.g. zone information). Check this entity before saving.", entity);
            }

            changedObjects.Add(entity);
            repairedCount++;
        }

        if (repairedCount > 0)
        {
            Undo.CollapseUndoOperations(undoGroup);

            changedObjects.Add(scenario);
            changedObjects.Add(graph);
            foreach (Object changedObject in changedObjects)
            {
                EditorUtility.SetDirty(changedObject);
            }
        }

        string skippedList = skipped.Count > 0 ? string.Join(", ", skipped) : "none";
        Debug.Log($"{LogPrefix} Graph \"{graph.name}\": repaired {repairedCount} one-sided Entity connection(s). Skipped: {skippedList}.", graph);
    }

    // Serialised entity data, excluding xNode's port/connection data
    private static JObject GetEntityData(EntityNode entity)
    {
        JObject data = JObject.Parse(EditorJsonUtility.ToJson(entity));
        data.Remove("ports");
        foreach (JProperty property in data.Properties())
        {
            if (property.Value is JObject wrapped)
            {
                wrapped.Remove("ports");
            }
        }

        return data;
    }
}
