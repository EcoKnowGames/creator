using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(EntityNode))]
public class EntityNodeEditor : NodeEditor
{
    private EntityNode _entityNode;

    private bool autoPlaced = false;
    private bool showMore = false;

    private Sprite entityIcon;

    public override void OnBodyGUI()
    {
        if (_entityNode == null)
            _entityNode = target as EntityNode;

        // Update serialized object's representation
        serializedObject.Update();

        // --- Sprite Preview ---
        if (entityIcon != null && entityIcon.texture != null)
        {
            Rect previewRect = GUILayoutUtility.GetRect(180, 60, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(previewRect, UnityEngine.Color.gray);

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
            EditorGUILayout.LabelField("Visual Options", EditorStyles.centeredGreyMiniLabel);
            entityIcon = EditorGUILayout.ObjectField(entityIcon, typeof(Sprite), true, GUILayout.Height(48), GUILayout.Width(48)) as Sprite;
            EditorGUILayout.Space();

            // Rates
            EditorGUILayout.LabelField("Rates", EditorStyles.centeredGreyMiniLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_growthRate"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_movementRate"));
            EditorGUILayout.Space();

            // Placement & Harvesting Options
            EditorGUILayout.LabelField("Options", EditorStyles.centeredGreyMiniLabel);
            autoPlaced = EditorGUILayout.Toggle("Auto Place?", autoPlaced);

            _entityNode.Harvestable = EditorGUILayout.Toggle("Harvestable?", _entityNode.Harvestable);
            if (_entityNode.Harvestable)
            {
                NodePort inputPort = _entityNode.GetInputPort("_harvestQuantity");
                NodeEditorGUILayout.PortField(inputPort);
                EditorGUILayout.Space();
            }

            // Introduction Options
            _entityNode.Introducable = EditorGUILayout.Toggle("Introducable?", _entityNode.Introducable);
            if (_entityNode.Introducable)
            {
                NodePort inputPort = _entityNode.GetInputPort("_introduceQuantity");
                NodeEditorGUILayout.PortField(inputPort); EditorGUILayout.Space();
            }

            // Warnings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Warning Ranges", EditorStyles.centeredGreyMiniLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_vulnerable"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_abundance"));
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }

}
