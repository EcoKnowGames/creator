using System.Linq;
using Glitchers.EcoKnow.Sandbox;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(EntityNode))]
public class EntityNodeEditor : NodeEditor
{
    private EntityNode _entityNode;

    private bool showMore = false;
    private bool showZones = false;

    private Sprite entityIcon;
    private ColourPaletteObject.ColourSwatch entityColour;

    public override void OnBodyGUI()
    {
        if (_entityNode == null)
            _entityNode = target as EntityNode;

        entityIcon = _entityNode.Icon;

        ColourPaletteObject.ColourSwatch[] swatches = GetSwatches();
        entityColour = swatches == null && _entityNode.ColourIndex < swatches.Length ? new ColourPaletteObject.ColourSwatch("black", Color.black) : swatches[_entityNode.ColourIndex];

        // Update serialized object's representation
        serializedObject.Update();

        // --- Sprite Preview ---
        if (entityIcon != null && entityIcon.texture != null)
        {
            Rect previewRect = GUILayoutUtility.GetRect(180, 60, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(previewRect, entityColour.colour);

            // Calculate UVs
            Rect texCoords = new Rect(
                entityIcon.rect.x / entityIcon.texture.width,
                entityIcon.rect.y / entityIcon.texture.height,
                entityIcon.rect.width / entityIcon.texture.width,
                entityIcon.rect.height / entityIcon.texture.height
            );

            // Calculate aspect ratios
            float spriteAspect = entityIcon.rect.width / entityIcon.rect.height;
            float containerAspect = previewRect.width / previewRect.height;

            // Centered sprite rect
            Rect centeredRect;
            if (spriteAspect > containerAspect)
            {
                float height = previewRect.width / spriteAspect;
                float yOffset = (previewRect.height - height) / 2f;
                centeredRect = new Rect(previewRect.x, previewRect.y + yOffset, previewRect.width, height);
            }
            else
            {
                float width = previewRect.height * spriteAspect;
                float xOffset = (previewRect.width - width) / 2f;
                centeredRect = new Rect(previewRect.x + xOffset, previewRect.y, width, previewRect.height);
            }

            // Draw sprite
            UnityEngine.Color originalColor = GUI.color;
            GUI.color = UnityEngine.Color.black;
            GUI.DrawTextureWithTexCoords(centeredRect, entityIcon.texture, texCoords);
            GUI.color = originalColor;
        }

        // --- Header: ID and Foldout ---
        EditorGUILayout.BeginHorizontal();
        {
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_id"), new GUIContent(" " + _entityNode.ID));
            showMore = EditorGUILayout.Foldout(showMore, "...");
        }
        EditorGUILayout.EndHorizontal();

        // --- Expanded Options ---
        if (showMore)
        {
            EditorGUILayout.Space();

            // Visual Options
            //Icon
            EditorGUILayout.LabelField("Visual Options", EditorStyles.centeredGreyMiniLabel);
            _entityNode.Icon = EditorGUILayout.ObjectField(entityIcon, typeof(Sprite), true, GUILayout.Height(48), GUILayout.Width(48)) as Sprite;
            EditorGUILayout.Space();

            //Colour
            if (swatches != null)
            {
                int colourIndex = _entityNode.ColourIndex;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<"))
                {
                    colourIndex = WrapIndex(colourIndex - 1, 0, swatches.Length);
                }

                entityColour = swatches[colourIndex];
                Rect colourRect = GUILayoutUtility.GetRect(100, 20, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(new Rect(colourRect.x, colourRect.y, colourRect.width, colourRect.height), entityColour.colour);

                var centeredStyle = GUI.skin.GetStyle("Label");
                centeredStyle.alignment = TextAnchor.UpperCenter;
                centeredStyle.fontStyle = FontStyle.Bold;
                centeredStyle.normal.textColor = Color.black;
                EditorGUI.LabelField(colourRect, new GUIContent(entityColour.name), centeredStyle);

                if (GUILayout.Button(">"))
                {
                    colourIndex = WrapIndex(colourIndex + 1, 0, swatches.Length);
                }
                EditorGUILayout.EndHorizontal();

                _entityNode.ColourIndex = colourIndex;
            }

            EditorGUILayout.Space();

            // Player Visibility
            EditorGUILayout.LabelField("Player Visibility", EditorStyles.centeredGreyMiniLabel);
            _entityNode.HiddenFromCellToken = DrawRightAlignedToggle("Hide Grid Token?", _entityNode.HiddenFromCellToken);
            _entityNode.HiddenFromEntityPanel = DrawRightAlignedToggle("Hide From Panel?", _entityNode.HiddenFromEntityPanel);


            // Rates
            EditorGUILayout.LabelField("Rates", EditorStyles.centeredGreyMiniLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_growthRate"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_movementRate"));
            EditorGUILayout.Space();

            // Placement & Harvesting Options
            EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);

            if (!_entityNode.CanAutoPlace())
            {
                EditorGUILayout.HelpBox("Tile Populations have been defined in the Map CSV - Auto Place will have no effect", MessageType.Warning);
                GUI.enabled = false;
            }
            _entityNode.AutoPlace = EditorGUILayout.Toggle("Auto Place?", _entityNode.AutoPlace);
            if (_entityNode.AutoPlace)
            {
                EditorGUILayout.HelpBox("Start Population applies to each tile", MessageType.Info);
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_startPopulation"));
            }

            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Harvest", EditorStyles.centeredGreyMiniLabel);
            _entityNode.CanHarvest = EditorGUILayout.Toggle("Can Harvest?", _entityNode.CanHarvest);
            if (_entityNode.CanHarvest)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_harvestLimit"), new GUIContent("Limit Per-Action"));

                NodePort inputPort = _entityNode.GetInputPort("_harvestQuantity");
                NodeEditorGUILayout.PortField(inputPort);
                EditorGUILayout.Space();
            }

            // Introduction Options
            EditorGUILayout.LabelField("Introduce", EditorStyles.centeredGreyMiniLabel);
            _entityNode.CanIntroduce = EditorGUILayout.Toggle("Can Introduce?", _entityNode.CanIntroduce);
            if (_entityNode.CanIntroduce)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_introduceLimit"), new GUIContent("Limit Per-Action"));

                NodePort inputPort = _entityNode.GetInputPort("_introduceQuantity");
                NodeEditorGUILayout.PortField(inputPort);
                EditorGUILayout.Space();
            }

            ZoneDef[] zoneDefs = GetZoneDefs();
            if (zoneDefs != null && zoneDefs.Length > 1)
            {
                _entityNode.AdjustZoneRates();

                EditorGUILayout.Space();
                showZones = EditorGUILayout.Foldout(showZones, "Zones");
                if (showZones)
                {
                    EditorGUILayout.Space();

                    foreach (EntityZoneInformation zoneRate in _entityNode.ZoneInformation)
                    {
                        if (zoneRate.ZoneID < 0 || zoneRate.ZoneID >= zoneDefs.Count())
                        {
                            continue;
                        }

                        ZoneDef zone = zoneDefs[zoneRate.ZoneID];

                        //Draw our title

                        GUILayout.BeginHorizontal();
                        Color zoneColour = Color.white;
                        ColorUtility.TryParseHtmlString(zone.Colour, out zoneColour);

                        Rect colourRect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(false));
                        EditorGUI.DrawRect(colourRect, zoneColour);
                        EditorGUILayout.LabelField($"{zone.Name} [{zone.ID}]", EditorStyles.boldLabel, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(100));
                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        zoneRate.GrowthRate = EditorGUILayout.FloatField("  Growth Rate", zoneRate.GrowthRate);
                        zoneRate.MovementRate = EditorGUILayout.FloatField("  Movement Rate", zoneRate.MovementRate);

                        EditorGUILayout.LabelField("Transitions", EditorStyles.centeredGreyMiniLabel);
                        foreach(ZoneDef other in zoneDefs)
                        {
                            bool shouldTransition = true;

                            if (other.ID != zone.ID)
                            {
                                shouldTransition = zoneRate.Transitions.Contains(other.ID);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{zone.Name} [{zone.ID}] -> {other.Name} [{other.ID}]");
                                shouldTransition = EditorGUILayout.Toggle(shouldTransition, GUILayout.Width(16));
                                EditorGUILayout.EndHorizontal();
                            }

                            if (shouldTransition)
                            {
                                if (!zoneRate.Transitions.Contains(other.ID))
                                {
                                    zoneRate.Transitions.Add(other.ID);
                                }
                            }
                            else
                            {
                                if (zoneRate.Transitions.Contains(other.ID))
                                {
                                    zoneRate.Transitions.Remove(other.ID);
                                }
                            }
                        }

                        zoneRate.Transitions = zoneRate.Transitions.OrderBy(x => x).ToList();
                        EditorGUILayout.Space(20);
                    }
                }
            }

            // Warnings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Warning Ranges", EditorStyles.centeredGreyMiniLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_vulnerable"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_abundance"));
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ports", EditorStyles.centeredGreyMiniLabel);

            if (_entityNode.CanHarvest)
            {
                NodePort inputPort = _entityNode.GetInputPort("_harvestQuantity");
                NodeEditorGUILayout.PortField(inputPort);
            }

            if (_entityNode.CanIntroduce)
            {
                NodePort inputPort = _entityNode.GetInputPort("_introduceQuantity");
                NodeEditorGUILayout.PortField(inputPort);
            }

        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }

    // Draws a labelled toggle as a row with the checkbox right-aligned, so longer
    // labels are not clipped by the checkbox in the narrow node body.
    private bool DrawRightAlignedToggle(string label, bool value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.FlexibleSpace();
        bool result = EditorGUILayout.Toggle(value, GUILayout.Width(16));
        EditorGUILayout.EndHorizontal();
        return result;
    }

    public int WrapIndex(int value, int min, int max)
    {
        if (value >= max)
        {
            value = 0;
        }
        else if (value < 0)
        {
            value = max - 1;
        }

        return value;
    }

    private ColourPaletteObject.ColourSwatch[] GetSwatches()
    {
        if (_entityNode.graph is ScenarioNodeGraph scenario)
        {
            ColourPaletteObject.ColourSwatch[] colourList = scenario.GetColours();
            if (colourList != null)
            {
                return colourList;
            }
        }

        return null;
    }

    private ZoneDef[] GetZoneDefs()
    {
        NodePort outputPort = _entityNode.GetInputPort("_id");
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

}
