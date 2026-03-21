using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelTerrain))]
public class VoxelTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var terrain = (VoxelTerrain)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        var hasSourceModel = serializedObject.FindProperty("sourceModel").objectReferenceValue != null;
        var hasChunks = terrain.transform.childCount > 0;

        if (hasSourceModel)
        {
            if (GUILayout.Button("Voxelize From Model", GUILayout.Height(30)))
            {
                Undo.RegisterFullObjectHierarchyUndo(terrain.gameObject, "Voxelize Terrain");

                var success = terrain.VoxelizeFromModel((progress, message) =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Voxelizing", message, progress))
                    {
                        // User cancelled — clean up
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
