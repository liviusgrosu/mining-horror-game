using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelTerrain))]
public class VoxelTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var sourceModelProp = serializedObject.FindProperty("sourceModel");
        var voxelsPerUnitProp = serializedObject.FindProperty("voxelsPerUnit");
        var sizeInChunksProp = serializedObject.FindProperty("sizeInChunks");
        var blockMaterialProp = serializedObject.FindProperty("blockMaterial");
        var mineRadiusProp = serializedObject.FindProperty("mineRadius");
        var oreProductName = serializedObject.FindProperty("oreProductName");

        var hasSourceModel = sourceModelProp.objectReferenceValue != null;

        EditorGUILayout.LabelField("Source Model", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(sourceModelProp);
        if (hasSourceModel)
            EditorGUILayout.PropertyField(voxelsPerUnitProp);

        EditorGUILayout.Space(5);
        if (!hasSourceModel)
        {
            EditorGUILayout.LabelField("Solid Block Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sizeInChunksProp);
            EditorGUILayout.PropertyField(blockMaterialProp);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Mining", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(mineRadiusProp);
        EditorGUILayout.PropertyField(oreProductName);

        serializedObject.ApplyModifiedProperties();

        // --- Editor Tools ---
        var terrain = (VoxelTerrain)target;
        var hasChunks = terrain.transform.childCount > 0;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        if (hasSourceModel)
        {
            if (GUILayout.Button("Voxelize From Model", GUILayout.Height(30)))
            {
                Undo.RegisterFullObjectHierarchyUndo(terrain.gameObject, "Voxelize Terrain");

                var success = terrain.VoxelizeFromModel((progress, message) =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Voxelizing", message, progress))
                    {
                        EditorUtility.ClearProgressBar();
                        terrain.ClearChunks();
                        return;
                    }
                });

                EditorUtility.ClearProgressBar();

                if (success)
                    EditorUtility.SetDirty(terrain);
            }
        }
        else
        {
            if (GUILayout.Button("Generate Solid Block", GUILayout.Height(30)))
            {
                Undo.RegisterFullObjectHierarchyUndo(terrain.gameObject, "Generate Solid Block");
                terrain.VoxelizeSolidBlock();
                EditorUtility.SetDirty(terrain);
            }
        }

        GUI.enabled = hasChunks;
        if (GUILayout.Button("Clear Chunks", GUILayout.Height(25)))
        {
            Undo.RegisterFullObjectHierarchyUndo(terrain.gameObject, "Clear Voxel Chunks");
            terrain.ClearChunks();
            EditorUtility.SetDirty(terrain);
        }
        GUI.enabled = true;

        if (hasChunks)
        {
            var chunkCount = 0;
            var totalVerts = 0;
            var totalTris = 0;
            foreach (var chunk in terrain.GetComponentsInChildren<VoxelChunk>())
            {
                chunkCount++;
                var mf = chunk.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    totalVerts += mf.sharedMesh.vertexCount;
                    totalTris += mf.sharedMesh.triangles.Length / 3;
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"Chunks: {chunkCount}  |  Vertices: {totalVerts:N0}  |  Triangles: {totalTris:N0}",
                MessageType.Info);
        }
    }
}
